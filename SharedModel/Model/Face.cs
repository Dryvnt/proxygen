using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SharedModel.Model;

public sealed class Face
{
    [Key]
    public int Id { get; init; }

    public int CardId { get; init; }
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
