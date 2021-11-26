using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Proxygen.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly IReadOnlyCollection<string> _interestingCards = new List<string>
        {
            "Snapcaster Mage",
            "2x Ponder",
            "2 Stomping Ground",
            "Dance of the Dead",
            "Jace, the Mind Sculptor",
            "Delver of Secrets",
            "Fire // Ice",
            "Fire",
            "Ice",
            "Akki Lavarunner",
            "Echo Mage",
            "Urza's Saga",
            "Asmoranomardicadaistinaculdacar",
            "Who",
        };

        private readonly ILogger<IndexModel> _logger;

        public string Decklist = string.Join("\n", _interestingCards);

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}