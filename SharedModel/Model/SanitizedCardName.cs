using System.ComponentModel.DataAnnotations;

namespace SharedModel.Model;

public sealed class SanitizedCardName
{
    [Key] [StringLength(256)] public string SanitizedName { get; init; } = null!;

    public int CardId { get; init; }
    public Card Card { get; init; } = null!;
}