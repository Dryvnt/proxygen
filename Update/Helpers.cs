using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SharedModel.Model;
using SharedModel.OracleJson;

namespace Update;

public static class Helpers
{
    private static readonly List<string> LayoutWhitelist = new()
    {
        "normal", "split", "flip", "transform", "modal_dfc", "meld", "leveler", "class", "saga", "adventure",
        "planar", "scheme",
    };

    private static readonly List<string> KnownLayouts = new()
    {
        "normal", "split", "flip", "transform", "modal_dfc", "meld", "leveler", "class", "saga", "adventure",
        "planar",
        "scheme", "vanguard", "token", "double_faced_token", "emblem", "augment", "host", "art_series",
        "double_sided",
    };

    public static async IAsyncEnumerable<Card> ConvertData(ILogger logger, IAsyncEnumerable<JsonCard> jsonCards,
        [EnumeratorCancellation] CancellationToken stoppingToken)
    {
        await foreach (var card in jsonCards.WithCancellation(stoppingToken))
        {
            if (!KnownLayouts.Contains(card.Layout)) logger.LogWarning("Unknown layout '{}", card.Layout);
            if (!LayoutWhitelist.Contains(card.Layout)) continue;
            yield return DataMapping.FromJson(card);
        }
    }

    public static async Task BuildCards(CardContext context, ILogger logger, IAsyncEnumerable<Card> cards,
        CancellationToken stoppingToken)
    {
        // It would probably be more efficient to manually upsert the cards, but that's complicated and the data set is so small.
        // TODO: Make this not nuke from orbit

        var cardsList = await cards.ToListAsync(stoppingToken);
        context.Cards.RemoveRange(context.Cards.ToList());
        await context.SaveChangesAsync(stoppingToken);
        await context.Cards.AddRangeAsync(cardsList, stoppingToken);
        await context.SaveChangesAsync(stoppingToken);
    }

    public static async Task BuildIndex(CardContext context, ILogger logger, CancellationToken stoppingToken)
    {
        // Nuke index and rebuild from scratch.
        context.Index.RemoveRange(context.Index.ToList());
        await context.SaveChangesAsync(stoppingToken);

        // Convert all faces to index entries
        var entries = context.Cards.AsEnumerable()
            .Select(c => new { Card = c, Names = Names.CardNames(c) })
            .SelectMany(p => p.Names.Select(n => new NameIndex { SanitizedName = n, Card = p.Card }));

        // If multiple cards share a name (see: Everythingamajig) just pick one of them, it's un-cards, we support those on a "if it works it works" basis.
        var uniqueEntries = entries.GroupBy(i => i.SanitizedName).Select(g => g.MinBy(i => i.Card.Id)!);

        await context.Index.AddRangeAsync(uniqueEntries, stoppingToken);
        await context.SaveChangesAsync(stoppingToken);
    }
}