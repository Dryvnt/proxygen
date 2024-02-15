using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Display : PageModel
{
    private const int CardLimit = 1000;
    private readonly IClock _clock;
    private readonly ILogger<Display> _logger;
    private readonly ProxygenContext _proxygenContext;

    public Display(ILogger<Display> logger, ProxygenContext proxygenContext, IClock clock)
    {
        _logger = logger;
        _proxygenContext = proxygenContext;
        _clock = clock;
    }

    public List<string> UnrecognizedCards { get; } = [];
    public List<Card> Cards { get; } = [];

    public async Task<IActionResult> OnGetAsync(
        string? decklist,
        CancellationToken cancellationToken
    )
    {
        if (decklist is null)
            return RedirectToPage("Index");

        var data = await Parser.ParseDecklist(decklist);

        var (missedNames, cardLookup) = await CardLookup(data.Keys, cancellationToken);

        foreach (var (name, amount) in data)
        {
            if (missedNames.Contains(name))
                continue;

            var card = cardLookup[name];
            // Sort faces for aesthetic value
            Cards.AddRange(Enumerable.Repeat(card, amount));
        }

        UnrecognizedCards.AddRange(missedNames);

        var record = new SearchRecord
        {
            When = _clock.GetCurrentInstant(),
            Cards = Cards,
            UnrecognizedCards = UnrecognizedCards.ToList(),
        };

        await _proxygenContext.AddAsync(record, cancellationToken);
        await _proxygenContext.SaveChangesAsync(cancellationToken);

        return Page();
    }

    private async Task<(HashSet<string>, Dictionary<string, Card>)> CardLookup(
        ICollection<string> names,
        CancellationToken cancellationToken = default
    )
    {
        var sanitizedNames = await _proxygenContext
            .SanitizedCardNames.Include(i => i.Card)
            .ThenInclude(c => c.Faces)
            .Where(s => names.Contains(s.Name))
            .ToArrayAsync(cancellationToken);

        var fetched = new Dictionary<string, Card>();

        // I HATE FUNNY CARDS - Makes us do this WEIRD data handling
        foreach (var nameGroup in sanitizedNames.GroupBy(s => s.Name))
        {
            var candidates = nameGroup.AsEnumerable();
            if (nameGroup.Count() > 1)
                candidates = candidates.Where(c => !c.Card.IsFunny);
            var candidate = candidates.First();
            fetched[candidate.Name] = candidate.Card;
        }

        var missedNames = names.Except(fetched.Keys).ToHashSet();

        return (missedNames, fetched);
    }
}
