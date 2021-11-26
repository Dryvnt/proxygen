using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Proxygen.Model;

namespace Proxygen
{
    public static class Parser
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

        
        private static (string, int) ParseLine(string line)
        {
            var matches = Regex.Match(line, @"^(([0-9]+)x?)?([a-zA-Z0-9\s]+)$", RegexOptions.Compiled);
            if(!matches.Success) throw new ArgumentException($"Line did not match {line}", nameof(line));
            var amountGroup = matches.Groups[2];
            var nameGroup = matches.Groups[3];

            var amount = amountGroup.Success ? int.Parse(amountGroup.Captures.First().Value) : 1;
            var name = nameGroup.Captures.First().Value;
            
            return (Sanitize(name), amount);
        }

        public static Task<IDictionary<string, int>> ParseDecklist(string decklist)
        {
            var lines = Regex.Split(decklist.Trim(), "\r\n|\r|\n");
            IDictionary<string, int> dict = lines.Select(Sanitize).Select(ParseLine).ToDictionary(p => p.Item1, p => p.Item2);
            return Task.FromResult(dict);
        }
    }
}