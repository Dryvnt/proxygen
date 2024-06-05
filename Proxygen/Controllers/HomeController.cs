using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Proxygen.Services;
using Proxygen.ViewModels.Home;
using SharedModel.Model;

namespace Proxygen.Controllers;

public class HomeController(ProxygenContext dbContext, IClock clock, CardSearcher cardSearcher)
    : Controller
{
    private static readonly IReadOnlyCollection<string> InterestingCards =
    [
        "Snapcaster Mage",
        "2x Ponder",
        "Dance of the Dead",
        "Leyline of the Guildpact",
        "2 Stomping Ground",
        "Arcane Proxy",
        "Dismember",
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
    ];
    private static readonly string DefaultDecklist = string.Join("\r\n", InterestingCards);
    private static IndexViewModel DefaultModel => new() { Decklist = DefaultDecklist };

    [HttpGet("/")]
    public IActionResult Index()
    {
        return View(DefaultModel);
    }

    [HttpGet("/search")]
    public async Task<IActionResult> SearchAsync(
        [Bind] IndexViewModel model,
        CancellationToken cancellationToken
    )
    {
        var searchResult = await cardSearcher.SearchAsync(model.Decklist, cancellationToken);

        foreach (var unrecognizedCard in searchResult.UnrecognizedFaces)
        {
            ModelState.AddModelError(
                nameof(IndexViewModel.Decklist),
                $"Unknown card: {unrecognizedCard}"
            );
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        return View("Search", new SearchViewModel { CardAmounts = searchResult.CardAmounts });
    }

    [HttpGet("/layout/{layout}")]
    public async Task<IActionResult> AllFlipAsync(CardLayout layout)
    {
        var allLayoutNames = dbContext.Cards.Where(c => c.CardLayout == layout).Select(c => c.Name);

        const int maxLength = 1024;

        var decklistBuilder = new StringBuilder();
        await foreach (var card in allLayoutNames.AsAsyncEnumerable())
        {
            var line = $"{card}\r\n";

            if (decklistBuilder.Length + line.Length > maxLength)
                break;

            decklistBuilder.Append(line);
        }

        return RedirectToAction("Search", new { Decklist = decklistBuilder.ToString() });
    }

    [HttpGet("/stats")]
    public async Task<IActionResult> Stats()
    {
        var now = clock.GetCurrentInstant();
        var oneWeekAgo = now.Minus(Duration.FromDays(7));
        var oneDayAgo = now.Minus(Duration.FromDays(1));

        var LastSevenDays = await dbContext
            .SearchRecords.Where(r => r.When >= oneWeekAgo)
            .Include(r => r.Cards)
            .AsNoTrackingWithIdentityResolution()
            .ToArrayAsync();
        var LastTwentyFourHours = await dbContext
            .SearchRecords.Where(r => r.When >= oneDayAgo)
            .Include(r => r.Cards)
            .AsNoTrackingWithIdentityResolution()
            .ToArrayAsync();
        var LastSevenUpdates = await dbContext
            .UpdateStatuses.OrderByDescending(u => u.Created)
            .Take(48)
            .AsNoTracking()
            .ToArrayAsync();

        return View(
            new StatsViewModel
            {
                LastSevenDays = LastSevenDays,
                LastTwentyFourHours = LastTwentyFourHours,
                LastSevenUpdates = LastSevenUpdates
            }
        );
    }
}
