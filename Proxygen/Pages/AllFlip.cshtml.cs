﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModel.Model;

namespace Proxygen.Pages;

public class AllFlip : PageModel
{
    private readonly CardContext _cardContext;
    private readonly ILogger<AllFlip> _logger;

    public AllFlip(ILogger<AllFlip> logger, CardContext cardContext)
    {
        _logger = logger;
        _cardContext = cardContext;
    }

    public IActionResult OnGet()
    {
        var allFlipNames = _cardContext.Cards.Where(c => c.Layout == Layout.Flip).Select(c => c.Name);
        var decklist = string.Join("\n", allFlipNames);
        return RedirectToPage(nameof(Display), new { Decklist = decklist });
    }
}