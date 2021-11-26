using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SharedModel
{
    public class Face
    {
        public Guid CardId { get; init; }
        public int Sequence { get; init; }
        public string Name { get; init; }
        public string? OracleText { get; init; }
        public string TypeLine { get; init; }
        public string? ManaCost { get; init; }
        public string? Power { get; init; }
        public string? Toughness { get; init; }
        public string? Loyalty { get; init; }

        public IEnumerable<string> ManaCostComponents
        {
            get
            {
                if (ManaCost is null) yield break;

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

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(OracleText)}: {OracleText}, {nameof(TypeLine)}: {TypeLine}, {nameof(ManaCost)}: {ManaCost}, {nameof(Power)}: {Power}, {nameof(Toughness)}: {Toughness}, {nameof(Loyalty)}: {Loyalty}";
        }
    }
}