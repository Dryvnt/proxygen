using NodaTime;

namespace Update.Options;

public class ProxygenUpdaterOptions
{
    public const string ProxygenUpdater = "Updater";

    public bool Enabled { get; set; }
    public Period UpdateInterval { get; set; } = Period.FromDays(1);
}
