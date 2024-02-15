using System.IO.Hashing;
using System.Text;
using SharedModel.Scryfall;

namespace Update.Extensions;

public static class HashAlgorithmExtensions
{
    public static void AppendScryfall(
        this NonCryptographicHashAlgorithm hashAlgorithm,
        ScryfallCard card
    )
    {
        hashAlgorithm.Append(card.Id.ToByteArray());
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.Name));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.TypeLine));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.Layout));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.ManaCost ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.OracleText ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.Power ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.Toughness ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.Loyalty ?? ""));
        if (card.Faces is not null)
        {
            foreach (var face in card.Faces)
            {
                hashAlgorithm.AppendScryfall(face);
            }
        }
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(card.SetType));
    }

    public static void AppendScryfall(
        this NonCryptographicHashAlgorithm hashAlgorithm,
        ScryfallFace face
    )
    {
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.Name));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.TypeLine));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.ManaCost ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.OracleText ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.Power ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.Toughness ?? ""));
        hashAlgorithm.Append(Encoding.UTF8.GetBytes(face.Loyalty ?? ""));
    }
}
