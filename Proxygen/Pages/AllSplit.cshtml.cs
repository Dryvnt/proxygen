using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModel.Model;

namespace Proxygen.Pages;

public class AllSplit : PageModel
{
    private readonly ILogger<AllSplit> _logger;
    private readonly ProxygenContext _proxygenContext;

    public AllSplit(ILogger<AllSplit> logger, ProxygenContext proxygenContext)
    {
        _logger = logger;
        _proxygenContext = proxygenContext;
    }

    public IActionResult OnGet()
    {
        var allSplitNames = _proxygenContext
            .Cards.Where(c => c.CardLayout == CardLayout.Split)
            .Select(c => c.Name);
        var decklist = string.Join("\n", allSplitNames);
        return RedirectToPage(nameof(Display), new { Decklist = decklist });
    }
}
