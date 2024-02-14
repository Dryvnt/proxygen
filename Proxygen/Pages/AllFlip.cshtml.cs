using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModel.Model;

namespace Proxygen.Pages;

public class AllFlip : PageModel
{
    private readonly ILogger<AllFlip> _logger;
    private readonly ProxygenContext _proxygenContext;

    public AllFlip(ILogger<AllFlip> logger, ProxygenContext proxygenContext)
    {
        _logger = logger;
        _proxygenContext = proxygenContext;
    }

    public IActionResult OnGet()
    {
        var allFlipNames = _proxygenContext
            .Cards.Where(c => c.CardLayout == CardLayout.Flip)
            .Select(c => c.Name);
        var decklist = string.Join("\n", allFlipNames);
        return RedirectToPage(nameof(Display), new { Decklist = decklist });
    }
}