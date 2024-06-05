using System.IO.Hashing;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SharedModel.Model;
using SharedModel.Scryfall;
using Update.Extensions;

namespace Update;

public static class DataMapping
{
    private static readonly IReadOnlyCollection<string> IgnoredLayouts = new HashSet<string>
    {
        "vanguard",
        "token",
        "double_faced_token",
        "emblem",
        "augment",
        "host",
        "art_series",
    };
    private static readonly IReadOnlyDictionary<string, CardLayout> CardLayoutMapping =
        new Dictionary<string, CardLayout>()
        {
            ["normal"] = CardLayout.Normal,
            ["leveler"] = CardLayout.Normal,
            ["class"] = CardLayout.Normal,
            ["saga"] = CardLayout.Normal,
            ["planar"] = CardLayout.Normal,
            ["scheme"] = CardLayout.Normal,
            ["transform"] = CardLayout.Normal,
            ["meld"] = CardLayout.Normal,
            ["modal_dfc"] = CardLayout.Normal,
            ["case"] = CardLayout.Normal,
            ["reversible_card"] = CardLayout.Normal,
            ["mutate"] = CardLayout.Normal,
            ["prototype"] = CardLayout.Normal,
            ["split"] = CardLayout.Split,
            ["adventure"] = CardLayout.Split,
            ["flip"] = CardLayout.Flip,
        };

    private static CardLayout ParseLayout(string layout)
    {
        if (CardLayoutMapping.TryGetValue(layout, out var cardLayout))
            return cardLayout;

        throw new InvalidOperationException($"Unsupported card layout whitelisted? {layout}");
    }

    public static async IAsyncEnumerable<Card> FromJsonStream(
        IAsyncEnumerable<ScryfallCard> cards,
        ILogger logger,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var card in cards.WithCancellation(cancellationToken))
        {
            if (IgnoredLayouts.Contains(card.Layout))
                continue;
            if (!CardLayoutMapping.Keys.Contains(card.Layout))
            {
                logger.LogWarning("Unknown layout '{}'", card.Layout);
                continue;
            }

            yield return FromJson(card);
        }
    }

    private static ulong JsonFingerPrint(ScryfallCard card)
    {
        var h = new XxHash64();
        h.AppendScryfall(card);
        return h.GetCurrentHashAsUInt64();
    }

    public static Card FromJson(ScryfallCard scryfallCard)
    {
        var layout = ParseLayout(scryfallCard.Layout);

        var faces = new List<Face>();
        if (scryfallCard.Faces?.Count > 0)
        {
            faces.AddRange(
                scryfallCard.Faces.Select(face => new Face
                {
                    CardId = scryfallCard.Id,
                    Name = face.Name,
                    OracleText = face.OracleText,
                    TypeLine = face.TypeLine,
                    ManaCost = face.ManaCost,
                    Power = face.Power,
                    Toughness = face.Toughness,
                    Loyalty = face.Loyalty,
                })
            );
        }
        else
        {
            faces.Add(
                new Face
                {
                    CardId = scryfallCard.Id,
                    Name = scryfallCard.Name,
                    OracleText = scryfallCard.OracleText,
                    TypeLine = scryfallCard.TypeLine,
                    ManaCost = scryfallCard.ManaCost,
                    Power = scryfallCard.Power,
                    Toughness = scryfallCard.Toughness,
                    Loyalty = scryfallCard.Loyalty,
                }
            );
        }

        var names = faces.Select(f => f.Name).Append(scryfallCard.Name);
        var sanitizedNames = names
            .Select(Names.Sanitize)
            .Distinct()
            .Select(n => new SanitizedCardName { Name = n, CardId = scryfallCard.Id })
            .ToList();

        return new Card
        {
            Id = scryfallCard.Id,
            Name = scryfallCard.Name,
            CardLayout = layout,
            Faces = faces,
            SanitizedNames = sanitizedNames,
            SourceDigest = JsonFingerPrint(scryfallCard),
            IsFunny = scryfallCard.SetType == "funny",
        };
    }
}
