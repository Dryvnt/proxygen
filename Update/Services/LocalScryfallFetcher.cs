using System.Runtime.CompilerServices;
using System.Text.Json;
using SharedModel.Scryfall;

namespace Update.Services;

public class LocalScryfallFetcher : IScryfallFetcher
{
    public async IAsyncEnumerable<ScryfallCard> FetchOracleCards(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await using var fs = File.OpenRead(
            @"C:\Users\dryvn\Downloads\oracle-cards-20240519210215.json"
        );
        await foreach (
            var q in JsonSerializer
                .DeserializeAsyncEnumerable<ScryfallCard>(fs, cancellationToken: cancellationToken)
                .OfType<ScryfallCard>()
                .WithCancellation(cancellationToken)
        )
        {
            yield return q;
        }
    }
}
