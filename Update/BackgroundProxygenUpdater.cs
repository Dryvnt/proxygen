using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Update.Options;
using Update.Services;

namespace Update;

public class BackgroundProxygenUpdater(
    IOptions<ProxygenUpdaterOptions> proxygenUpdaterOptions,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<BackgroundProxygenUpdater> logger
) : BackgroundService
{
    private readonly ProxygenUpdaterOptions _options = proxygenUpdaterOptions.Value;

    private readonly Duration _updateFrequency = Duration.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            Duration? waitDuration = null;
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
                waitDuration = await updateHandler.HandleUpdateAsync(stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unexpected error during background update loop");
                waitDuration ??= _options.UpdateInterval.ToDuration();
            }
            logger.LogInformation("Sleeping {} until next update", waitDuration);
            await Task.Delay(waitDuration.Value.ToTimeSpan(), stoppingToken);
        }
    }
}
