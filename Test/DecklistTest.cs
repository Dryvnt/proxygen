using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proxygen;
using Update;
using Xunit;

namespace Test;

public class DecklistTest
{
    public static TheoryData<string, IEnumerable<(string, int)>> ExampleDeckLists =>
        new()
        {
            { "", Array.Empty<(string, int)>() },
            { "Snapcaster Mage", new[] { ("Snapcaster Mage", 1) } },
            { "Snapcaster Mage\n", new[] { ("Snapcaster Mage", 1) } },
            { "\n\n\n\nSnapcaster Mage\n\n\n\n\n", new[] { ("Snapcaster Mage", 1) } },
            { "2 Snapcaster Mage", new[] { ("Snapcaster Mage", 2) } },
            { "2x Snapcaster Mage", new[] { ("Snapcaster Mage", 2) } },
            {
                "3 Island\nPonder\nFire // Ice",
                new[] { ("Island", 3), ("Ponder", 1), ("Fire // Ice", 1) }
            },
        };

    [Theory]
    [MemberData(nameof(ExampleDeckLists))]
    public void Basic(string decklist, IEnumerable<(string, int)> expected)
    {
        var parsedExpected = expected.ToDictionary(p => Names.Sanitize(p.Item1), p => p.Item2);

        var parsed = Parser.ParseDecklist(decklist);

        Assert.Equal(parsedExpected, parsed);
    }
}
