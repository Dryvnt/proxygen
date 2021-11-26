using System;
using System.Collections.Generic;
using System.Linq;
using Proxygen.Model;

namespace Cli
{
    public static class DataMapping
    {
        public static Card FromJson(JsonCard jsonCard)
        {
            var layout = jsonCard.Layout switch
            {
                "normal" or "leveler" or "class" or "saga" or "planar" or "scheme" or "transform" or "meld" or
                    "modal_dfc" => Layout
                        .Normal,
                "split" or "adventure" => Layout.Split,
                "flip" => Layout.Flip,
                _ => throw new NotImplementedException($"Unsupported card layout whitelisted? {jsonCard.Layout}"),
            };

            var faces = new List<Face>();
            if (jsonCard.Faces?.Any() ?? false)
            {
                var i = 0;
                foreach (var face in jsonCard.Faces)
                {
                    faces.Add(new Face
                    {
                        Sequence = i,
                        Name = face.Name,
                        OracleText = face.OracleText,
                        TypeLine = face.TypeLine,
                        ManaCost = face.ManaCost,
                        Power = face.Power,
                        Toughness = face.Toughness,
                        Loyalty = face.Loyalty,
                    });
                    i++;
                }
            }
            else
            {
                faces.Add(new Face
                    {
                        Sequence = 0,
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

            return new Card { Id = jsonCard.Id, Name = jsonCard.Name, Layout = layout, Faces = faces };
        }
    }
}