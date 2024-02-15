using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Stats(ProxygenContext db, IClock clock) : PageModel
{
    public IReadOnlyCollection<UpdateStatus> LastSevenUpdates = Array.Empty<UpdateStatus>();
    public IReadOnlyCollection<SearchRecord> LastSevenDays = Array.Empty<SearchRecord>();
    public IReadOnlyCollection<SearchRecord> LastTwentyFourHours = Array.Empty<SearchRecord>();

    public async Task<IActionResult> OnGetAsync()
    {
        var now = clock.GetCurrentInstant();
        var oneWeekAgo = now.Minus(Duration.FromDays(7));
        var oneDayAgo = now.Minus(Duration.FromDays(1));

        LastSevenDays = await db
            .SearchRecords.Where(r => r.When >= oneWeekAgo)
            .Include(r => r.Cards)
            .AsNoTrackingWithIdentityResolution()
            .ToArrayAsync();
        LastTwentyFourHours = await db
            .SearchRecords.Where(r => r.When >= oneDayAgo)
            .Include(r => r.Cards)
            .AsNoTrackingWithIdentityResolution()
            .ToArrayAsync();

        LastSevenUpdates = await db
            .UpdateStatuses.OrderByDescending(u => u.Created)
            .Take(48)
            .AsNoTracking()
            .ToArrayAsync();

        return Page();
    }
}
