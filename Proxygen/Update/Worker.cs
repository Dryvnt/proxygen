using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proxygen.Model;
using Proxygen.OracleJson;

namespace Proxygen.Update
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Worker(IServiceScopeFactory serviceScopeFactory, ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private readonly TimeSpan _updateFrequency = TimeSpan.FromDays(1);

        private async Task WaitUntilMigrated(CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<CardContext>();
            while ((await context.Database.GetPendingMigrationsAsync(stoppingToken)).Any())
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        
        private async Task<TimeSpan> UntilNextUpdate(CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<CardContext>();

            var latest = await context.Updates.OrderBy(u => u.When).LastOrDefaultAsync(stoppingToken);

            if (latest is null) return TimeSpan.Zero;
            
            if (latest.Status is UpdateStatus.Begun)
            {
                _logger.LogError("An earlier update failed. Setting status to failure. {}", latest);
                latest.Status = UpdateStatus.Failure;
                await context.SaveChangesAsync(stoppingToken);
            }

            var timeSinceLatest = DateTime.UtcNow - latest.When;

            var untilNext = _updateFrequency - timeSinceLatest;
            return untilNext < TimeSpan.Zero ? TimeSpan.Zero : untilNext;
        }

        private async IAsyncEnumerable<JsonCard> FetchData([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            using var client = _httpClientFactory.CreateClient();

            var bulkInformationRaw = await client.GetStreamAsync("https://api.scryfall.com/bulk-data", stoppingToken);
            var bulkInformationWrapper = await JsonSerializer.DeserializeAsync<BulkInformationWrapper>(bulkInformationRaw, cancellationToken: stoppingToken);

            if (bulkInformationWrapper is null) throw new NotImplementedException("Could not parse bulk data?");

            var oracleInformation = bulkInformationWrapper.BulkInformations.First(i => i.Type == "oracle_cards");
            
            _logger.LogError("Oracle information: {}", oracleInformation);
            
            var oracleRaw = await client.GetStreamAsync(oracleInformation.DownloadUri, stoppingToken);
            var jsonStream = JsonSerializer.DeserializeAsyncEnumerable<JsonCard>(oracleRaw, cancellationToken: stoppingToken);

            await foreach (var card in jsonStream.WithCancellation(stoppingToken))
            {
                if (card is null) throw new NotImplementedException();
                yield return card!;
            }
        }

        private async Task PerformUpdate(CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<CardContext>();

            var update = new Model.Update
            {
                Id = Guid.NewGuid(),
                When = DateTime.UtcNow,
                Status = UpdateStatus.Begun,
            };

            await context.Updates.AddAsync(update, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);

            try
            {
                _logger.LogInformation("Fetching data");
                var jsonData = FetchData(stoppingToken);
                var cardData = Helpers.ConvertData(_logger, jsonData, stoppingToken);
                await using (var transaction = await context.Database.BeginTransactionAsync(stoppingToken))
                {
                    _logger.LogInformation("Inserting cards");
                    await Helpers.BuildCards(context, _logger, cardData, stoppingToken);
                    _logger.LogInformation("Building index");
                    await Helpers.BuildIndex(context, _logger, stoppingToken);

                    await transaction.CommitAsync(stoppingToken);
                }
                update.Status = UpdateStatus.Success;
            }
            catch (Exception e)
            {
                _logger.LogError("Update failed. Exception: {}", e.Message);
                update.Status = UpdateStatus.Failure;
            }
            finally
            {
                _logger.LogInformation("Committing changes");
                await context.SaveChangesAsync(stoppingToken);
            }
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitUntilMigrated(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var untilNext = await UntilNextUpdate(stoppingToken);
                await Task.Delay(untilNext, stoppingToken);
                await PerformUpdate(stoppingToken);
            }
        }
    }
}