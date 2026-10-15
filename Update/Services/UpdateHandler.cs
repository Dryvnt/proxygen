using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using SharedModel.Model;
using Update.Options;

namespace Update.Services;

public class UpdateHandler(
    ILogger<UpdateHandler> logger,
    IOptions<ProxygenUpdaterOptions> options,
    ProxygenContext db,
    IScryfallFetcher scryfallFetcher,
    IClock clock
) : IUpdateHandler
{
    private readonly ProxygenUpdaterOptions _options = options.Value;

    private async Task<Duration> TimeSinceLastUpdate(CancellationToken cancellationToken)
    {
        var failed = await db
            .UpdateStatuses.Where(u => u.StatusState == UpdateStatusState.Begun)
            .ExecuteUpdateAsync(
                p => p.SetProperty(u => u.StatusState, UpdateStatusState.Failure),
                cancellationToken
            );
        if (failed > 0)
        {
            logger.LogError("{} previous update did not finish, so we set them to error", failed);
        }

        var latest = await db
            .UpdateStatuses.OrderBy(u => u.Created)
            .Where(u => u.StatusState == UpdateStatusState.Success)
            .AsNoTracking()
            .LastOrDefaultAsync(cancellationToken);

        if (latest is null)
            return _options.UpdateInterval.ToDuration();

        return clock.GetCurrentInstant() - latest.Created;
    }

    private async Task PerformUpdateInner(
        IAsyncEnumerable<Card> cards,
        CancellationToken cancellationToken
    )
    {
        var oldDigests = await db
            .Cards.Select(c => new { ScryfallId = c.Id, c.SourceDigest })
            .AsNoTracking()
            .ToDictionaryAsync(t => t.ScryfallId, t => t.SourceDigest, cancellationToken);
        var unhandled = oldDigests.Keys.ToHashSet();
        var changed = new List<Card>();

        await foreach (var card in cards.WithCancellation(cancellationToken))
        {
            if (!oldDigests.TryGetValue(card.Id, out var oldDigest))
            {
                await db.Cards.AddAsync(card, cancellationToken);
                continue;
            }
            unhandled.Remove(card.Id);

            if (oldDigest == card.SourceDigest)
            {
                continue;
            }

            changed.Add(card);
        }

        // Faces and sanitized names are keyed by name, which can change between updates.
        // Replace them wholesale instead of updating; the card row itself is kept so its
        // search record links survive.
        var changedIds = changed.Select(c => c.Id).ToHashSet();
        await db.Set<Face>()
            .Where(f => changedIds.Contains(f.CardId))
            .ExecuteDeleteAsync(cancellationToken);
        await db
            .SanitizedCardNames.Where(n => changedIds.Contains(n.CardId))
            .ExecuteDeleteAsync(cancellationToken);
        foreach (var card in changed)
        {
            db.Entry(card).State = EntityState.Modified;
            await db.AddRangeAsync(card.Faces, cancellationToken);
            await db.AddRangeAsync(card.SanitizedNames, cancellationToken);
        }

        // id is unhandled: card is not present in oracle data and should be deleted
        await db
            .Cards.Where(c => unhandled.Contains(c.Id))
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PerformUpdate(CancellationToken cancellationToken)
    {
        logger.LogInformation("Performing card data update");

        var update = new UpdateStatus
        {
            Created = clock.GetCurrentInstant(),
            StatusState = UpdateStatusState.Begun,
        };

        await db.UpdateStatuses.AddAsync(update, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        using var _ = logger.BeginScope(update.Id);

        try
        {
            var jsonData = scryfallFetcher.FetchOracleCards(cancellationToken);
            var cardData = DataMapping.FromJsonStream(jsonData, logger, cancellationToken);

            await using (
                var transaction = await db.Database.BeginTransactionAsync(cancellationToken)
            )
            {
                // Status is saved together with the card changes, so both commit or neither does
                update.StatusState = UpdateStatusState.Success;
                await PerformUpdateInner(cardData, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "card data update failed");

            // Drop pending card changes so only the failure status is saved
            db.ChangeTracker.Clear();
            update.StatusState = UpdateStatusState.Failure;
            db.UpdateStatuses.Update(update);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception saveException)
            {
                logger.LogError(saveException, "Failed to save update failure status");
            }
        }

        logger.LogInformation("Finished card data update (status {})", update.StatusState);
    }

    public async Task<Duration> HandleUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return _options.UpdateInterval.ToDuration();

        var sinceLast = await TimeSinceLastUpdate(cancellationToken);
        var untilNext = Duration.Max(
            Duration.Zero,
            _options.UpdateInterval.ToDuration() - sinceLast
        );
        if (untilNext > Duration.Zero)
            return untilNext;

        await PerformUpdate(cancellationToken);
        return _options.UpdateInterval.ToDuration();
    }
}
