using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SharedModel.Model;

[Index(nameof(Name))]
[PrimaryKey(nameof(CardId), nameof(Name))]
public class SanitizedCardName
{
    [StringLength(256)]
    public required string Name { get; init; }

    public Guid CardId { get; init; }
    public Card Card { get; init; } = null!;
}
