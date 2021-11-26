using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Proxygen.Model
{
    public class CardContext : DbContext
    {
        public DbSet<Card> Cards { get; init; } = null!;
        public DbSet<NameIndex> Index { get; init; } = null!;

        public DbSet<Update> Updates { get; init; } = null!;

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

        public CardContext(DbContextOptions<CardContext> options) : base(options)
        {
        }
    }
}