﻿using System.Text.Json.Serialization;

namespace SharedModel.Scryfall;

public sealed record ScryfallFace(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type_line")] string TypeLine,
    [property: JsonPropertyName("mana_cost")] string? ManaCost,
    [property: JsonPropertyName("oracle_text")] string? OracleText,
    [property: JsonPropertyName("power")] string? Power,
    [property: JsonPropertyName("toughness")] string? Toughness,
    [property: JsonPropertyName("loyalty")] string? Loyalty
);
