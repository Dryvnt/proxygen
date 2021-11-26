using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SharedModel;

namespace Proxygen.Pages
{
    public class AllSplit : PageModel
    {
        private readonly CardContext _cardContext;
        private readonly ILogger<AllSplit> _logger;

        public AllSplit(ILogger<AllSplit> logger, CardContext cardContext)
        {
            _logger = logger;
            _cardContext = cardContext;
        }
        
        public IActionResult OnGet()
        {
            var allSplitNames = _cardContext.Cards.Where(c => c.Layout == Layout.Split).Select(c => c.Name);
            var decklist = string.Join("\n", allSplitNames);
            return RedirectToPage("Display", new { Decklist = decklist });
        }
    }
}