using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace SharedModel.Model;

[Index(nameof(Name))]
[PrimaryKey(nameof(CardId), nameof(Name))]
public sealed class Face
{
    public required Guid CardId { get; init; }
    public Card Card { get; init; } = null!;

    [StringLength(256)]
    public required string Name { get; init; }

    [StringLength(1024)]
    public string? OracleText { get; init; }

    [StringLength(64)]
    public string? TypeLine { get; init; }

    [StringLength(64)]
    public string? ManaCost { get; init; }

    [StringLength(8)]
    public string? Power { get; init; }

    [StringLength(8)]
    public string? Toughness { get; init; }

    [StringLength(8)]
    public string? Loyalty { get; init; }

    [NotMapped]
    public IEnumerable<string> ManaCostComponents
    {
        get
        {
            if (ManaCost is null)
                yield break;

            foreach (Match match in Regex.Matches(ManaCost, @"\{.*?\}"))
            {
                var part = match.Value;
                if (part.Length == 3)
                    yield return part.Trim('{', '}');
                else
                    yield return part;
            }
        }
    }
}
