using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Proxygen.Model;

namespace Proxygen.Pages
{
    public class Display : PageModel
    {
        private readonly CardContext _cardContext;
        private readonly ILogger<Display> _logger;

        public Display(ILogger<Display> logger, CardContext cardContext)
        {
            _logger = logger;
            _cardContext = cardContext;
        }

        public List<Card> Cards { get; } = new();
        public HashSet<string> UnrecognizedCards = new();

        public async Task<IActionResult> OnGetAsync(string decklist)
        {
            var data = await Parser.ParseDecklist(decklist);
            
            var (missedNames, cardLookup) = CardLookup(data.Keys);
            foreach (var name in missedNames) UnrecognizedCards.Add(name);

            foreach (var (name, amount) in data)
            {
                if(missedNames.Contains(name)) continue;
                var card = cardLookup[name];
                // Sort faces for aesthetic value
                card.SortFaces();
                for (var i = 0; i < amount; i++)
                {
                    Cards.Add(card);
                }
            }

            return Page();
        }

        private (HashSet<string>, Dictionary<string, Card>) CardLookup(ICollection<string> names)
        {
            var fetched = _cardContext.Index.Include(i => i.Card).ThenInclude(c => c.Faces)
                .Where(i => names.Contains(i.SanitizedName)).ToDictionary(i => i.SanitizedName, i => i.Card);

            // Ensure we got all names
            var missedNames = names.Except(fetched.Keys).ToHashSet();

            return (missedNames, fetched);
        }
    }
}