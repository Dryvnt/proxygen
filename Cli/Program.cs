using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Proxygen;
using Proxygen.Model;
using Proxygen.OracleJson;
using Proxygen.Update;

namespace Cli
{
    internal class Program
    {

        public static async Task<ICollection<JsonCard>> ReadJson(Stream input)
        {
            return await JsonSerializer.DeserializeAsync<List<JsonCard>>(input) ??
                            throw new NotImplementedException();
        }

        /// <summary>
        ///     Simple tool for importing an oracle json dump from Scryfall into the database
        /// </summary>
        /// <param name="inputFile">File to read JSON from. If not provided, json will be read from stdin</param>
        /// <param name="filterTo">Write the parsed JSON to this file before inserting into DB</param>
        public static async Task Main(FileInfo? inputFile, FileInfo? filterTo)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger<Program>();
            var stoppingToken = CancellationToken.None;
            
            logger.LogInformation("Opening input file");
            await using var input = inputFile?.OpenRead() ?? Console.OpenStandardInput();

            var jsonCards = await ReadJson(input);
            
            if (filterTo is not null)
            {
                logger.LogInformation("Writing filtered JSON to file");
                await using var filterOut = filterTo.OpenWrite();
                await JsonSerializer.SerializeAsync(filterOut, jsonCards, cancellationToken: stoppingToken);
            }
            
            var cards = Helpers.ConvertData(logger, jsonCards.ToAsyncEnumerable(), stoppingToken);

            var dbOptions = new DbContextOptionsBuilder<CardContext>();
            dbOptions.UseNpgsql("Host=localhost;Port=5433;Database=proxygen;Username=proxygen;Password=password");
            await using var context = new CardContext(dbOptions.Options);
            Console.WriteLine("Migrating DB");
            await context.Database.MigrateAsync(cancellationToken: stoppingToken);
            await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

            Console.WriteLine("Inserting cards");
            await Helpers.BuildCards(context, logger, cards, stoppingToken);

            Console.WriteLine("Building index");
            await Helpers.BuildIndex(context, logger, stoppingToken);

            Console.WriteLine("Committing changes to DB");
            await transaction.CommitAsync(stoppingToken);
        }
    }
}