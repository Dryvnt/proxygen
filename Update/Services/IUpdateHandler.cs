using NodaTime;

namespace Update.Services;

public interface IUpdateHandler
{
    public Task<Duration> HandleUpdateAsync(CancellationToken cancellationToken = default);
}
