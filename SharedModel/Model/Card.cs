using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SharedModel.Model;

public enum CardLayout
{
    Normal = 1, // Print each face one-by-one
    Split = 2, // Print each face on the same card
    Flip = 3, // Print each face on the same card, rotate the second face
}

[Index(nameof(Name))]
[Index(nameof(Id))]
public sealed class Card
{
    [Key]
    public required Guid Id { get; init; }

    // "longest name ever" elemental is 141 characters, choose nearest power of two because why not
    [StringLength(256)]
    public required string Name { get; init; }

    public required CardLayout CardLayout { get; init; }
    public required ICollection<SanitizedCardName> SanitizedNames { get; init; }

    public required bool IsFunny { get; init; }

    public required ulong SourceDigest { get; set; }
    public ICollection<Face> Faces { get; set; } = null!;

    public ICollection<SearchRecord> SearchRecords { get; init; } = null!;
}
