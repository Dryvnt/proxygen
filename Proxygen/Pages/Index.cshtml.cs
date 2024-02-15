using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proxygen.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private static readonly IReadOnlyCollection<string> InterestingCards = new List<string>
    {
        "Snapcaster Mage",
        "2x Ponder",
        "2 Stomping Ground",
        "Arcane Proxy",
        "Dance of the Dead",
        "Jace, the Mind Sculptor",
        "Delver of Secrets",
        "Fire // Ice",
        "Fire",
        "Ice",
        "Illuna, Apex of Wishes",
        "Akki Lavarunner",
        "Echo Mage",
        "Urza's Saga",
        "Asmoranomardicadaistinaculdacar",
        "Who",
    };

    private readonly ILogger<IndexModel> _logger = logger;

    public string Decklist = string.Join("\n", InterestingCards);

    public void OnGet() { }
}
