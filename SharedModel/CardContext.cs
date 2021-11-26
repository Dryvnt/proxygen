using System.IO;
using Microsoft.EntityFrameworkCore;

namespace SharedModel
{
    public class CardContext : DbContext
    {
        public DbSet<Card> Cards { get; init; }
        public DbSet<NameIndex> Index { get; init; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>()
                .HasMany(c => c.Faces)
                .WithOne()
                .HasForeignKey(f => f.CardId)
                .IsRequired();

            modelBuilder.Entity<Face>()
                .HasKey(f => new { f.CardId, f.Sequence });

            modelBuilder.Entity<NameIndex>()
                .HasOne(i => i.Card)
                .WithMany()
                .IsRequired();
        }
    }

    public class SqliteCardContext : CardContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tmp = Path.GetTempPath();
            var path = Path.Combine(tmp, "proxygen.sqlite");
            optionsBuilder.UseSqlite($"Data Source={path}");
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}