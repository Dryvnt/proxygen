using Microsoft.EntityFrameworkCore;
using NodaTime;
using SharedModel.Model;

namespace Proxygen.Services;

public class CardSearcher(ProxygenContext db, IClock clock)
{
    public record SearchResult(
        IReadOnlyDictionary<Card, int> CardAmounts,
        IReadOnlyCollection<string> UnrecognizedFaces
    );

    private async Task<(
        IReadOnlyDictionary<string, Card> nameDict,
        IReadOnlyCollection<string> missedNames
    )> FetchCards(string[] names, CancellationToken cancellationToken)
    {
        // Fetch cards from DB
        var cardsByName = await db
            .SanitizedCardNames.Include(i => i.Card)
            .ThenInclude(c => c.Faces)
            .Where(s => names.Contains(s.Name))
            .GroupBy(s => s.Name)
            .ToArrayAsync(cancellationToken);

        // Post-process required to handle FUNNY cards :/
        var nameDict = cardsByName
            .Select(g => g.FirstOrDefault(c => !c.Card.IsFunny) ?? g.First())
            .ToDictionary(c => c.Name, c => c.Card);

        var missedNames = names.Except(nameDict.Keys).ToHashSet();

        return (nameDict, missedNames);
    }

    public async Task<SearchResult> SearchAsync(
        string decklist,
        CancellationToken cancellationToken = default
    )
    {
        var parsedDecklist = Parser.ParseDecklist(decklist);

        cancellationToken.ThrowIfCancellationRequested();

        var (cardsByName, missed) = await FetchCards(
            parsedDecklist.Keys.ToArray(),
            cancellationToken
        );

        var cardAmounts = parsedDecklist
            .Where(c => cardsByName.ContainsKey(c.Key))
            .GroupBy(c => cardsByName[c.Key])
            .ToDictionary(g => g.Key, g => g.Select(c => c.Value).Sum());

        var record = new SearchRecord
        {
            When = clock.GetCurrentInstant(),
            Cards = [.. cardAmounts.Keys],
            UnrecognizedCards = [.. missed],
        };

        await db.AddAsync(record, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new(cardAmounts, missed);
    }
}
