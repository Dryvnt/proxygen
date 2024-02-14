using SharedModel.Model;

namespace SharedModel.OracleJson;

public static class DataMapping
{
    public static Card FromJson(JsonCard jsonCard)
    {
        var layout = jsonCard.Layout switch
        {
            "normal"
            or "leveler"
            or "class"
            or "saga"
            or "planar"
            or "scheme"
            or "transform"
            or "meld"
            or "modal_dfc"
                => CardLayout.Normal,
            "split" or "adventure" => CardLayout.Split,
            "flip" => CardLayout.Flip,
            _
                => throw new NotImplementedException(
                    $"Unsupported card layout whitelisted? {jsonCard.Layout}"
                ),
        };

        var faces = new List<Face>();
        if (jsonCard.Faces?.Any() ?? false)
        {
            foreach (var face in jsonCard.Faces)
            {
                faces.Add(
                    new Face
                    {
                        Name = face.Name,
                        OracleText = face.OracleText,
                        TypeLine = face.TypeLine,
                        ManaCost = face.ManaCost,
                        Power = face.Power,
                        Toughness = face.Toughness,
                        Loyalty = face.Loyalty,
                    }
                );
            }
        }
        else
        {
            faces.Add(
                new Face
                {
                    Name = jsonCard.Name,
                    OracleText = jsonCard.OracleText,
                    TypeLine = jsonCard.TypeLine,
                    ManaCost = jsonCard.ManaCost,
                    Power = jsonCard.Power,
                    Toughness = jsonCard.Toughness,
                    Loyalty = jsonCard.Loyalty,
                }
            );
        }

        return new Card
        {
            ScryfallId = jsonCard.Id,
            Name = jsonCard.Name,
            CardLayout = layout,
            Faces = faces,
        };
    }
}
