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
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardLayout = CardLayout.Normal,
            Name = "Foobar",
            Faces = new List<Face>
            {
                new() { Name = "Foo", CardId = cardId, },
                new() { Name = "Bar", CardId = cardId, },
            },
            SanitizedNames = null!,
            SourceDigest = 0,
            IsFunny = false,
        };

        var expected = new[] { "foobar", "foo", "bar" }.Select(Names.Sanitize);

        Assert.Equal(expected, Names.CardNames(card));
    }
}
