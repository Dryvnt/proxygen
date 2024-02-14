using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SharedModel.Model;

public enum CardLayout
{
    Normal = 1, // Print each face one-by-one
    Split = 2, // Print each face on the same card
    Flip = 3, // Print each face on the same card, rotate the second face
}

[Index(nameof(Name), IsUnique = true)]
public sealed class Card
{
    [Key] public int Id { get; init; }

    public required Guid ScryfallId { get; init; }

    // "longest name ever" elemental is 141 characters, choose nearest power of two because why not
    [StringLength(256)] public required string Name { get; init; }

    public required CardLayout CardLayout { get; init; }
    public ICollection<Face> Faces { get; set; } = new List<Face>();

    public ICollection<SanitizedCardName> SanitizedCardNames { get; init; } =
        new List<SanitizedCardName>();

    public ICollection<SearchRecord> SearchRecords { get; init; } = new List<SearchRecord>();

    // Underlying DB representation has no concept of order, so we dictate it here.
    public void SortFaces()
    {
        Faces = Faces.OrderBy(face => face.Id).ToList();
    }
}