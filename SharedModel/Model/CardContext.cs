using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SharedModel.Model
{
    public class CardContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public CardContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}