using System.Collections.Generic;
using System.Linq;
using Proxygen;
using Proxygen.Model;
using Xunit;

namespace Test
{
    public class CardNamesTest
    {
        [Fact]
        public void Basic()
        {
            var card = new Card
            {
                Name = "Foobar",
                Faces = new List<Face>
                {
                    new()
                    {
                        Name = "Foo",
                    },
                    new()
                    {
                        Name = "Bar",
                    },
                },
            };

            var expected = new[] { "foobar", "foo", "bar" }.Select(Parser.Sanitize);

            Assert.Equal(expected, Parser.CardNames(card));
        }
    }
}