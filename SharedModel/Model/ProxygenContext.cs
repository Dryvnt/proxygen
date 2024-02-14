using Microsoft.EntityFrameworkCore;

namespace SharedModel.Model;

public sealed class ProxygenContext(DbContextOptions<ProxygenContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<SanitizedCardName> SanitizedCardNames => Set<SanitizedCardName>();
    public DbSet<UpdateStatus> UpdateStatuses => Set<UpdateStatus>();
    public DbSet<SearchRecord> SearchRecords => Set<SearchRecord>();
}
