using System.ComponentModel.DataAnnotations;

namespace Proxygen.Model
{
    public class NameIndex
    {
        [Key] public string SanitizedName { get; init; }

        public Card Card { get; init; }

        public override string ToString()
        {
            return $"{nameof(NameIndex)} ({nameof(SanitizedName)}: {SanitizedName}, {nameof(Card)}: {Card})";
        }
    }
}