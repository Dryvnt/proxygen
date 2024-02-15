using SharedModel.Model;

namespace Update;

public static class Names
{
    private static readonly HashSet<char> KeepChar = [' ', '/', ',', '\'', '-',];

    public static string Sanitize(string name)
    {
        var lower = name.ToLowerInvariant();

        return new string(lower.Where(c => !KeepChar.Contains(c)).ToArray());
    }

    public static IEnumerable<string> CardNames(Card card)
    {
        yield return Sanitize(card.Name);
        foreach (var face in card.Faces)
            yield return Sanitize(face.Name);
    }
}
