using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Display : PageModel
{
    private const int CardLimit = 1000;
    private readonly CardContext _cardContext;
    private readonly ILogger<Display> _logger;

    public Display(ILogger<Display> logger, CardContext cardContext)
    {
        _logger = logger;
        _cardContext = cardContext;
    }

    public List<Card> Cards { get; } = new();

    public async Task<IActionResult> OnGetAsync(string? decklist)
    {
        if (decklist is null)
            return BadRequest("No decklist");

        var data = await Parser.ParseDecklist(decklist);

        if (data.Keys.Count >= CardLimit || data.Values.Sum() >= CardLimit)
            return BadRequest("Too many cards");

        var (missedNames, cardLookup) = CardLookup(data.Keys);

        var record = new Record
        {
            Id = Guid.NewGuid(),
            When = DateTime.UtcNow,
            Cards = Cards,
            UnrecognizedCards = missedNames.ToList(),
        };

        await _cardContext.AddAsync(record);
        await _cardContext.SaveChangesAsync();

        if (missedNames.Any())
            return BadRequest($"Unrecognized cards:\n{string.Join("\n", missedNames)}");

        foreach (var (name, amount) in data)
        {
            var card = cardLookup[name];
            // Sort faces for aesthetic value
            card.SortFaces();
            Cards.AddRange(Enumerable.Repeat(card, amount));
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