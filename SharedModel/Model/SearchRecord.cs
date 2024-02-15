using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace SharedModel.Model;

public sealed class SearchRecord
{
    [Key]
    public int Id { get; init; }

    public required Instant When { get; init; }
    public ICollection<Card> Cards { get; init; } = null!;
    public List<string> UnrecognizedCards { get; init; } = [];
}
