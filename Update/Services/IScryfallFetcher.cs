using SharedModel.Scryfall;

namespace Update.Services;

public interface IScryfallFetcher
{
    public IAsyncEnumerable<ScryfallCard> FetchOracleCards(
        CancellationToken cancellationToken = default
    );
}
