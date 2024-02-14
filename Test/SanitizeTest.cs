using Update;
using Xunit;

namespace Test;

public class SanitizeTest
{
    [Theory]
    [InlineData("Snapcaster Mage", "snapcastermage")]
    [InlineData("Jace, the Mind Sculptor", "jacethemindsculptor")]
    [InlineData("Fire // Ice", "fireice")]
    public void Basic(string name, string expected)
    {
        Assert.Equal(expected, Names.Sanitize(name));
    }
}
