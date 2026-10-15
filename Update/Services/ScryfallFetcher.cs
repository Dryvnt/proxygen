using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using SharedModel.Scryfall;

namespace Update.Services;

public class ScryfallFetcher(HttpClient httpClient) : IScryfallFetcher
{
    public async IAsyncEnumerable<ScryfallCard> FetchOracleCards(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var bulkInformationRaw = await httpClient.GetStreamAsync(
            "https://api.scryfall.com/bulk-data",
            cancellationToken
        );
        var bulkInformationWrapper = await JsonSerializer.DeserializeAsync<ScryfallBulkWrapper>(
            bulkInformationRaw,
            cancellationToken: cancellationToken
        );

        if (bulkInformationWrapper is null)
            throw new NotImplementedException("Could not parse bulk data?");

        var oracleInformation = bulkInformationWrapper.Bulks.First(i => i.Type == "oracle_cards");

        // Bulk files are gzip-compressed JSON Lines (one card object per line)
        await using var oracleRaw = await httpClient.GetStreamAsync(
            oracleInformation.JsonlDownloadUri,
            cancellationToken
        );
        await using var oracleDecompressed = new GZipStream(oracleRaw, CompressionMode.Decompress);
        var jsonStream = JsonSerializer.DeserializeAsyncEnumerable<ScryfallCard>(
            oracleDecompressed,
            topLevelValues: true,
            cancellationToken: cancellationToken
        );

        await foreach (var card in jsonStream)
        {
            if (card is null)
                throw new NotImplementedException();
            yield return card;
        }
    }
}
