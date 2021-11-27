using System.ComponentModel.DataAnnotations;

namespace SharedModel.Model
{
    public class NameIndex
    {
        [Key] public string SanitizedName { get; init; } = null!;

        public Card Card { get; init; } = null!;

        public override string ToString()
        {
            return $"{nameof(NameIndex)} ({nameof(SanitizedName)}: {SanitizedName}, {nameof(Card)}: {Card})";
        }
    }
}