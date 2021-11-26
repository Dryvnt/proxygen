using System.Collections.Generic;
using System.Linq;
using Proxygen;
using Xunit;

namespace Test
{
    public class DecklistTest
    {
        public static TheoryData<string, IEnumerable<(string, int)>> ExampleDecklists =>
            new()
            {
                { "Snapcaster Mage", new[] { ("Snapcaster Mage", 1) } },
                { "Snapcaster Mage\n", new[] { ("Snapcaster Mage", 1) } },
                { "\n\n\n\nSnapcaster Mage\n\n\n\n\n", new[] { ("Snapcaster Mage", 1) } },
                { "2 Snapcaster Mage", new[] { ("Snapcaster Mage", 2) } },
                { "2x Snapcaster Mage", new[] { ("Snapcaster Mage", 2) } },
                { "3 Island\nPonder\nFire // Ice", new[] { ("Island", 3), ("Ponder", 1), ("Fire // Ice", 1) } },
            };

        [Theory]
        [MemberData(nameof(ExampleDecklists))]
        public void Basic(string decklist, IEnumerable<(string, int)> expected)
        {
            var parsedExpected = expected.ToDictionary(p => Parser.Sanitize(p.Item1), p => p.Item2);

            var parsed = Parser.ParseDecklist(decklist);

            Assert.Equal(parsedExpected, parsed);
        }
    }
}