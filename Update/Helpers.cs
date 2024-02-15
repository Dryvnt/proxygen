using Microsoft.Extensions.Logging;
using SharedModel.Model;

namespace Update;

public static class Helpers
{
    public static async Task BuildCards(
        ProxygenContext context,
        ILogger logger,
        IAsyncEnumerable<Card> cards,
        CancellationToken stoppingToken
    )
    {
        // It would probably be more efficient to manually upsert the cards, but that's complicated and the data set is so small.
        // TODO: Make this not nuke from orbit

        var cardsList = await cards.ToListAsync(stoppingToken);
        context.Cards.RemoveRange(context.Cards.ToList());
        await context.SaveChangesAsync(stoppingToken);
        await context.Cards.AddRangeAsync(cardsList, stoppingToken);
        await context.SaveChangesAsync(stoppingToken);
    }

    public static async Task BuildIndex(
        ProxygenContext context,
        ILogger logger,
        CancellationToken stoppingToken
    ) { }
}
