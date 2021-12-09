using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedModel.Model;
using SharedModel.OracleJson;
using Update;

namespace Cli;

internal class Program
{
    private static async Task<ICollection<JsonCard>> ReadJson(Stream input)
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
        var services = new ServiceCollection();
        services.AddLogging(options => options.AddConsole());
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var connectionString = config.GetConnectionString("Database");

        services.AddDbContext<CardContext>(options => options.UseNpgsql(connectionString));
        var provider = services.BuildServiceProvider();
        var stoppingToken = CancellationToken.None;

        var logger = provider.GetRequiredService<ILogger<Program>>();
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

        await using var serviceScope = provider.CreateAsyncScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<CardContext>();

        Console.WriteLine("Migrating DB");
        await context.Database.MigrateAsync(stoppingToken);
        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

        Console.WriteLine("Inserting cards");
        await Helpers.BuildCards(context, logger, cards, stoppingToken);

        Console.WriteLine("Building index");
        await Helpers.BuildIndex(context, logger, stoppingToken);

        Console.WriteLine("Committing changes to DB");
        await transaction.CommitAsync(stoppingToken);
    }
}