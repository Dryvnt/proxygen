﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SharedModel.Model;

namespace Proxygen.Pages;

public class Display : PageModel
{
    private readonly CardContext _cardContext;
    private readonly ILogger<Display> _logger;
    public HashSet<string> UnrecognizedCards = new();

    public Display(ILogger<Display> logger, CardContext cardContext)
    {
        _logger = logger;
        _cardContext = cardContext;
    }

    public List<Card> Cards { get; } = new();

    public async Task<IActionResult> OnGetAsync(string? decklist)
    {
        decklist ??= "";
        using var scope = _logger.BeginScope(new { Decklist = decklist.Replace("\r", "").Replace("\n", "\\n") });

        IDictionary<string, int> data;
        try
        {
            data = await Parser.ParseDecklist(decklist);
        }
        catch (Exception)
        {
            _logger.LogError("Decklist parsing failed");
            throw;
        }

        var (missedNames, cardLookup) = CardLookup(data.Keys);
        foreach (var name in missedNames) UnrecognizedCards.Add(name);

        foreach (var (name, amount) in data)
        {
            if (missedNames.Contains(name)) continue;
            var card = cardLookup[name];
            // Sort faces for aesthetic value
            card.SortFaces();
            for (var i = 0; i < amount; i++) Cards.Add(card);
        }

        var record = new Record
        {
            Id = Guid.NewGuid(),
            When = DateTime.UtcNow,
            Cards = Cards,
            UnrecognizedCards = missedNames.ToList(),
        };

        await _cardContext.AddAsync(record);
        await _cardContext.SaveChangesAsync();

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