using System;
using System.Collections.Generic;
using System.Linq;
using SharedModel.Model;
using Update;
using Xunit;

namespace Test;

public class CardNamesTest
{
    [Fact]
    public void Basic()
    {
        var card = new Card
        {
            ScryfallId = Guid.NewGuid(),
            CardLayout = CardLayout.Normal,
            Name = "Foobar",
            Faces = new List<Face>
            {
                new() { Name = "Foo" },
                new() { Name = "Bar" },
            },
        };

        var expected = new[] { "foobar", "foo", "bar" }.Select(Names.Sanitize);

        Assert.Equal(expected, Names.CardNames(card));
    }
}