using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedModel.Model;
using SharedModel.OracleJson;

namespace Update
{
    public class Worker : BackgroundService
    {
        private readonly bool _enabled;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Worker> _logger;
        private readonly TimeSpan _minimumWait;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly TimeSpan _updateFrequency = TimeSpan.FromDays(1);

        public Worker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<Worker> logger,
            IHttpClientFactory httpClientFactory)
        {
            var conf = configuration.GetSection("Updater");
            _enabled = conf?.GetValue<bool?>("Enabled") ?? false;

            var minimumWaitSeconds = conf?.GetValue<int?>("MinimumDelay") ?? 0;
            _minimumWait = TimeSpan.FromSeconds(minimumWaitSeconds);

            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

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

            if (latest is null) return _minimumWait;

            if (latest.Status is UpdateStatus.Begun)
            {
                _logger.LogError("An earlier update failed. Setting status to failure. {}", latest);
                latest.Status = UpdateStatus.Failure;
                await context.SaveChangesAsync(stoppingToken);
            }

            var timeSinceLatest = DateTime.UtcNow - latest.When;

            var untilNext = _updateFrequency - timeSinceLatest;
            return untilNext < _minimumWait ? _minimumWait : untilNext;
        }

        private async IAsyncEnumerable<JsonCard> FetchData([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            using var client = _httpClientFactory.CreateClient();

            var bulkInformationRaw = await client.GetStreamAsync("https://api.scryfall.com/bulk-data", stoppingToken);
            var bulkInformationWrapper =
                await JsonSerializer.DeserializeAsync<BulkInformationWrapper>(bulkInformationRaw,
                    cancellationToken: stoppingToken);

            if (bulkInformationWrapper is null) throw new NotImplementedException("Could not parse bulk data?");

            var oracleInformation = bulkInformationWrapper.BulkInformations.First(i => i.Type == "oracle_cards");

            var oracleRaw = await client.GetStreamAsync(oracleInformation.DownloadUri, stoppingToken);
            var jsonStream =
                JsonSerializer.DeserializeAsyncEnumerable<JsonCard>(oracleRaw, cancellationToken: stoppingToken);

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

            var update = new SharedModel.Model.Update
            {
                Id = Guid.NewGuid(),
                When = DateTime.UtcNow,
                Status = UpdateStatus.Begun,
            };

            using var _ = _logger.BeginScope(update.Id);
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

                _logger.LogInformation("Update success");
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
                _logger.LogInformation("Changed committed");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitUntilMigrated(stoppingToken);

            if (!_enabled) return;

            while (!stoppingToken.IsCancellationRequested)
            {
                var untilNext = await UntilNextUpdate(stoppingToken);
                if (untilNext > TimeSpan.Zero) _logger.LogInformation("Waiting {} until next update", untilNext);
                await Task.Delay(untilNext, stoppingToken);
                await PerformUpdate(stoppingToken);
            }
        }
    }
}