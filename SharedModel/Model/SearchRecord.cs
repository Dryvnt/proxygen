using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace SharedModel.Model;

public sealed class SearchRecord
{
    [Key]
    public int Id { get; init; }
    public required Instant When { get; init; }
    public required ICollection<Card> Cards { get; init; }
    public required ICollection<string> UnrecognizedCards { get; init; }
}
