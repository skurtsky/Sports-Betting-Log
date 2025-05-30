using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Models;

namespace SportsBettingTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Bet> Bets { get; set; }
        public DbSet<SportLeague> SportLeagues { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
              // Add some default sport leagues
            modelBuilder.Entity<SportLeague>().HasData(
                new SportLeague { Id = 1, Name = "NFL", Description = "National Football League" },
                new SportLeague { Id = 2, Name = "NBA", Description = "National Basketball Association" },
                new SportLeague { Id = 3, Name = "MLB", Description = "Major League Baseball" },
                new SportLeague { Id = 4, Name = "NHL", Description = "National Hockey League" },
                new SportLeague { Id = 5, Name = "UFC", Description = "Ultimate Fighting Championship" },
                new SportLeague { Id = 6, Name = "Soccer - Premier League", Description = "English Premier League" },
                new SportLeague { Id = 7, Name = "Soccer - MLS", Description = "Major League Soccer" }
            );
        }
    }
}
