using System.Collections.Generic;
using System.Linq;
using SharedModel.Model;

namespace Update
{
    public static class Names
    {
        private static bool KeepChar(char c)
        {
            var strip = new[] { ' ', '/', ',', '\'', '-' };
            return !strip.Contains(c);
        }

        public static string Sanitize(string name)
        {
            var lower = name.ToLowerInvariant();

            var stripped = new string(lower.Where(KeepChar).ToArray());
            return stripped;
        }

        public static IEnumerable<string> CardNames(Card card)
        {
            yield return Sanitize(card.Name);
            foreach (var face in card.Faces) yield return Sanitize(face.Name);
        }
    }
}