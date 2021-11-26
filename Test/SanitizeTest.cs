using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Parsing;
using Xunit;

namespace Test
{
    public class SanitizeTest
    {
        [Theory]
        [InlineData("Snapcaster Mage", "snapcastermage")]
        [InlineData("Jace, the Mind Sculptor", "jacethemindsculptor")]
        [InlineData("Fire // Ice", "fireice")]
        public void Basic(string name, string expected)
        {
            Assert.Equal(expected, Parser.Sanitize(name));
        }

        [Theory(Skip = "Long runtime. Only run manually.")]
        [MemberData(nameof(AllCardNames))]
        public void AlwaysAscii(string name)
        {
            var sanitized = Parser.Sanitize(name);
            
            // Weird way to assert string is ascii
            Assert.Equal(sanitized.Length, Encoding.UTF8.GetByteCount(sanitized));
            
        }

        public static TheoryData<string> AllCardNames
        {
            get
            {
                var jsonRaw = File.OpenRead("all-names.json");
                var names = JsonSerializer.Deserialize<List<string>>(jsonRaw);
                Debug.Assert(names is not null);
                var data = new TheoryData<string>();
                foreach (var name in names)
                {
                    data.Add(name);
                }
                return data;
            }
        }
    }
}