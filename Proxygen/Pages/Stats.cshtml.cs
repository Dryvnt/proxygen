using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Stats : PageModel
{
    private readonly IClock _clock;
    private readonly ILogger<Stats> _logger;
    private readonly ProxygenContext _proxygenContext;

    public readonly List<UpdateStatus> LastSevenUpdates = new();

    public int FailLastSevenDays;
    public int FailLastTwentyFourHours;

    public int RequestLastTwentyFourHours;
    public int RequestsLastSevenDays;

    public Stats(ILogger<Stats> logger, ProxygenContext proxygenContext, IClock clock)
    {
        _logger = logger;
        _proxygenContext = proxygenContext;
        _clock = clock;
    }

    public Task OnGetAsync()
    {
        var now = _clock.GetCurrentInstant();
        var oneWeekAgo = now.Minus(Duration.FromDays(7));
        var oneDayAgo = now.Minus(Duration.FromDays(1));

        var lastSevenDays = _proxygenContext.SearchRecords.Where(r => r.When >= oneWeekAgo);
        var lastTwentyFourHours = _proxygenContext.SearchRecords.Where(r => r.When >= oneDayAgo);

        RequestsLastSevenDays = lastSevenDays.Count();
        RequestLastTwentyFourHours = lastTwentyFourHours.Count();

        FailLastSevenDays = lastSevenDays.Count(r => r.UnrecognizedCards.Any());
        FailLastTwentyFourHours = lastTwentyFourHours.Count(r => r.UnrecognizedCards.Any());

        LastSevenUpdates.AddRange(
            _proxygenContext.UpdateStatuses.OrderByDescending(u => u.Created).Take(7)
        );

        return Task.CompletedTask;
    }
}