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
            .Cards.Select(c => new { ScryfallId = c.Id, c.SourceDigest, })
            .ToDictionaryAsync(t => t.ScryfallId, t => t.SourceDigest, cancellationToken);
        var unhandled = oldDigests.Keys.ToHashSet();

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

            db.Cards.Update(card);
        }

        // id is unhandled: card is not present in oracle data and should be deleted
        await db
            .Cards.Where(c => unhandled.Contains(c.Id))
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
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
                await PerformUpdateInner(cardData, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            update.StatusState = UpdateStatusState.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "card data update failed");
            update.StatusState = UpdateStatusState.Failure;
        }
        finally
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Finished card data update (status {})", update.StatusState);
        }
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
