using System.Text.Json.Serialization;

namespace SharedModel.OracleJson;

public record BulkInformationWrapper(
    [property: JsonPropertyName("data")] List<BulkInformation> BulkInformations
);

public record BulkInformation(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("download_uri")] Uri DownloadUri
);
