using Microsoft.EntityFrameworkCore;
using WorldCup2026_Web.Models;

namespace WorldCup2026_Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veri tabanında oluşacak tablolarımız
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Stadium> Stadiums { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Anecdote> Anecdotes { get; set; } = null!;
        public DbSet<Prediction> Predictions { get; set; } = null!; // Yeni tahmin tablomuzu buraya güvenle ekledik
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Match tablosundaki ilişkilerin SQL Server'da çakışmaması için kurallar (Kritik Korunma Alanı)
            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Stadium)
                .WithMany()
                .HasForeignKey(m => m.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}