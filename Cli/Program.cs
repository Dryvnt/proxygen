using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Parsing;
using SharedModel;

namespace Cli
{
    internal static class Program
    {
        private static readonly List<string> LayoutWhitelist = new()
        {
            "normal", "split", "flip", "transform", "modal_dfc", "meld", "leveler", "class", "saga", "adventure",
            "planar", "scheme",
        };

        private static readonly List<string> KnownLayouts = new()
        {
            "normal", "split", "flip", "transform", "modal_dfc", "meld", "leveler", "class", "saga", "adventure",
            "planar",
            "scheme", "vanguard", "token", "double_faced_token", "emblem", "augment", "host", "art_series",
            "double_sided",
        };

        public static async Task<ICollection<Card>> ReadData(Stream input)
        {
            var jsonCards = await JsonSerializer.DeserializeAsync<List<JsonCard>>(input) ??
                            throw new NotImplementedException();

            var unknownLayouts = jsonCards.Select(c => c.Layout).Distinct().Where(l => !KnownLayouts.Contains(l));
            foreach (var unknown in unknownLayouts) Console.WriteLine($"WARNING - UNKNOWN LAYOUT '{unknown}");
            var filteredCards = jsonCards.Where(c => LayoutWhitelist.Contains(c.Layout)).ToList();

            return filteredCards.Select(DataMapping.FromJson).ToList();
        }

        public static async Task BuildCards(ICollection<Card> cards, CardContext context)
        {
            // It would probably be more efficient to manually upsert the cards, but that's complicated and the data set is so small.
            // TODO: Make this not nuke from orbit

            context.Cards.RemoveRange(context.Cards.ToList());
            await context.SaveChangesAsync();
            await context.Cards.AddRangeAsync(cards);
            await context.SaveChangesAsync();
        }

        public static async Task BuildIndex(CardContext context)
        {
            // Nuke index and rebuild from scratch.
            context.Index.RemoveRange(context.Index.ToList());
            await context.SaveChangesAsync();

            // Convert all faces to index entries
            var entries = context.Cards.AsEnumerable()
                .Select(c => new { Card = c, Names = Parser.CardNames(c) })
                .SelectMany(p => p.Names.Select(n => new NameIndex { SanitizedName = n, Card = p.Card }));

            // If multiple cards share a name (see: Everythingamajig) just pick one of them, it's un-cards, we support those on a "if it works it works" basis.
            var uniqueEntries = entries.GroupBy(i => i.SanitizedName).Select(g => g.MinBy(i => i.Card.Id)!);

            await context.Index.AddRangeAsync(uniqueEntries);
            await context.SaveChangesAsync();
        }


        /// <summary>
        ///     Simple tool for importing an oracle json dump from Scryfall into the database
        /// </summary>
        /// <param name="inputFile">File to read JSON from. If not provided, json will be read from stdin</param>
        public static async Task Main(FileInfo? inputFile)
        {
            Console.WriteLine("Opening input file");
            var input = inputFile?.OpenRead() ?? Console.OpenStandardInput();

            var cards = await ReadData(input);

            await using var context = new SqliteCardContext();
            Console.WriteLine("Migrating DB");
            await context.Database.MigrateAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            Console.WriteLine("Inserting cards");
            await BuildCards(cards, context);

            Console.WriteLine("Building index");
            await BuildIndex(context);

            Console.WriteLine("Committing changes to DB");
            await transaction.CommitAsync();
        }
    }
}