using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Stats : PageModel
{
    private readonly CardContext _cardContext;
    private readonly ILogger<Stats> _logger;

    public int FailLastSevenDays;
    public int FailLastTwentyFourHours;

    public List<SharedModel.Model.Update> LastSevenUpdates;

    public int RequestLastTwentyFourHours;
    public int RequestsLastSevenDays;

    public Stats(ILogger<Stats> logger, CardContext cardContext)
    {
        _logger = logger;
        _cardContext = cardContext;
    }

    public Task OnGetAsync()
    {
        var lastSevenDays = _cardContext.Records.Where(r => r.When > DateTime.UtcNow - TimeSpan.FromDays(7));
        var lastTwentyFourHours =
            _cardContext.Records.Where(r => r.When > DateTime.UtcNow - TimeSpan.FromHours(24));

        RequestsLastSevenDays = lastSevenDays.Count();
        RequestLastTwentyFourHours = lastTwentyFourHours.Count();

        FailLastSevenDays = lastSevenDays.Count(r => r.UnrecognizedCards.Any());
        FailLastTwentyFourHours = lastTwentyFourHours.Count(r => r.UnrecognizedCards.Any());

        LastSevenUpdates = _cardContext.Updates.OrderByDescending(u => u.When).Take(7).ToList();

        return Task.CompletedTask;
    }
}