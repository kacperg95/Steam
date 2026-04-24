using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Repository.Entites;
using System.Linq.Expressions;

namespace Repository
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Developer> Developers => Set<Developer>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Screenshot> Screenshots => Set<Screenshot>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.GameId);
                entity.Property(e => e.GameId)
                    .ValueGeneratedNever();

                entity.HasIndex(g => g.LastUpdated).HasDatabaseName("IX_Games_LastUpdated");
                entity.HasIndex(g => g.ReleaseDate).HasDatabaseName("IX_Games_ReleaseDate");

                entity.HasIndex(g => g.TotalReviews).HasDatabaseName("IX_Games_TotalReviews");
                entity.HasIndex(g => g.ReviewScore).HasDatabaseName("IX_Games_ReviewScore");
                entity.HasIndex(g => new { g.TotalReviews, g.ReviewScore }).HasDatabaseName("IX_Games_TotalReviews_ReviewScore");


                entity.HasMany(g => g.Achievements).WithOne(a => a.Game).HasForeignKey(a => a.GameId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(g => g.Reviews).WithOne(r => r.Game).HasForeignKey(r => r.GameId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(g => g.Screenshots).WithOne(s => s.Game).HasForeignKey(s => s.GameId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(g => g.Movies).WithOne(m => m.Game).HasForeignKey(m => m.GameId).OnDelete(DeleteBehavior.Cascade);

                ConfigureManyToMany(entity, g => g.Genres, "GameGenres", "GenreId");
                ConfigureManyToMany(entity, g => g.Categories, "GameCategories", "CategoryId");
                ConfigureManyToMany(entity, g => g.Tags, "GameTags", "TagId");
                ConfigureManyToMany(entity, g => g.Publishers, "GamePublishers", "PublisherId");
                ConfigureManyToMany(entity, g => g.Developers, "GameDevelopers", "DeveloperId");
            });

            modelBuilder.Entity<Publisher>(e =>
            {
                e.HasIndex(p => p.Name).IsUnique().HasDatabaseName("IX_Publishers_Name");
            });

            modelBuilder.Entity<Developer>(e =>
            {
                e.HasIndex(d => d.Name).IsUnique().HasDatabaseName("IX_Developers_Name");
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.HasIndex(t => t.Name).IsUnique().HasDatabaseName("IX_Tags_Name");
            });

            modelBuilder.Entity<Movie>(e =>
            {
                e.Property(m => m.Id).ValueGeneratedNever();
                e.HasKey(m => new { m.Id, m.GameId });
            });

            modelBuilder.Entity<Category>().Property(e => e.CategoryId).ValueGeneratedNever();
            modelBuilder.Entity<Genre>().Property(e => e.GenreId).ValueGeneratedNever();
            modelBuilder.Entity<Review>().Property(e => e.Id).ValueGeneratedNever();

        }

        private void ConfigureManyToMany<TRelated>(EntityTypeBuilder<Game> entity,
        Expression<Func<Game, IEnumerable<TRelated>>> navigation,
        string tableName, string foreignKey) where TRelated : class
        {
            entity.HasMany(navigation!)
                  .WithMany("Games")
                  .UsingEntity<Dictionary<string, object>>(
                      tableName,
                      j => j.HasOne<TRelated>().WithMany().HasForeignKey(foreignKey),
                      j => j.HasOne<Game>().WithMany().HasForeignKey("GameId"),
                      j => { j.HasIndex(foreignKey).HasDatabaseName($"IX_{tableName}_{foreignKey}"); }
                  );
        }
    }
}