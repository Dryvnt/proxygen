using System.Text.Json.Serialization;

namespace SharedModel.Scryfall;

public sealed record ScryfallBulk(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("jsonl_download_uri")] Uri JsonlDownloadUri
);
