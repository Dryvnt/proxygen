using System.Text.RegularExpressions;
using Update;

namespace Proxygen;

public static partial class Parser
{
    private static (string, int) ParseLine(string line)
    {
        var matches = CardLineRegex().Match(line);
        if (!matches.Success)
            throw new ArgumentException($"Line did not match {line}", nameof(line));
        var amountGroup = matches.Groups[2];
        var nameGroup = matches.Groups[3];

        var amount = amountGroup.Success ? int.Parse(amountGroup.Captures.First().Value) : 1;
        var name = nameGroup.Captures.First().Value;

        return (Names.Sanitize(name), amount);
    }

    public static IReadOnlyDictionary<string, int> ParseDecklist(string decklist)
    {
        var lines = decklist.Split(
            NewlineSeperator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        var dict = new Dictionary<string, int>();
        foreach (var (name, amount) in lines.Select(Names.Sanitize).Select(ParseLine))
        {
            if (dict.TryGetValue(name, out var current))
                dict[name] = current + amount;
            else
                dict[name] = amount;
        }

        return dict;
    }

    [GeneratedRegex(@"^(([0-9]+)x?)?([a-zA-Z0-9\s]+)$", RegexOptions.Compiled)]
    private static partial Regex CardLineRegex();

    private static readonly string[] NewlineSeperator = ["\r\n", "\n", "\r",];
}
