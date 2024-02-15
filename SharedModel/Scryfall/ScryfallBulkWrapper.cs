using System.Text.Json.Serialization;

namespace SharedModel.Scryfall;

public sealed record ScryfallBulkWrapper(
    [property: JsonPropertyName("data")] List<ScryfallBulk> Bulks
);
