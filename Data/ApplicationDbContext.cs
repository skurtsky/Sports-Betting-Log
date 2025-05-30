using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Models;

namespace SportsBettingTracker.Data
{    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Bet> Bets { get; set; }
        public DbSet<SportLeague> SportLeagues { get; set; }
        public DbSet<BetTypeConfiguration> BetTypeConfigurations { get; set; }
          protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
              // Add some default sport leagues with display order
            modelBuilder.Entity<SportLeague>().HasData(
                new SportLeague { Id = 1, Name = "NFL", Description = "National Football League", DisplayOrder = 1, IsActive = true },
                new SportLeague { Id = 2, Name = "NBA", Description = "National Basketball Association", DisplayOrder = 2, IsActive = true },
                new SportLeague { Id = 3, Name = "MLB", Description = "Major League Baseball", DisplayOrder = 3, IsActive = true },
                new SportLeague { Id = 4, Name = "NHL", Description = "National Hockey League", DisplayOrder = 4, IsActive = true },
                new SportLeague { Id = 5, Name = "UFC", Description = "Ultimate Fighting Championship", DisplayOrder = 5, IsActive = true },
                new SportLeague { Id = 6, Name = "Soccer - Premier League", Description = "English Premier League", DisplayOrder = 6, IsActive = true },
                new SportLeague { Id = 7, Name = "Soccer - MLS", Description = "Major League Soccer", DisplayOrder = 7, IsActive = true }
            );
            
            // Add bet type configurations
            modelBuilder.Entity<BetTypeConfiguration>().HasData(
                new BetTypeConfiguration { Id = 1, BetType = BetType.Moneyline, DisplayName = "Moneyline", DisplayOrder = 1, IsActive = true, Description = "Bet on which team wins the game outright" },
                new BetTypeConfiguration { Id = 2, BetType = BetType.Spread, DisplayName = "Spread", DisplayOrder = 2, IsActive = true, Description = "Bet on the margin of victory" },
                new BetTypeConfiguration { Id = 3, BetType = BetType.OverUnder, DisplayName = "Over/Under", DisplayOrder = 3, IsActive = true, Description = "Bet on the total combined score" },
                new BetTypeConfiguration { Id = 4, BetType = BetType.Prop, DisplayName = "Prop Bet", DisplayOrder = 4, IsActive = true, Description = "Bet on specific events within the game" },
                new BetTypeConfiguration { Id = 5, BetType = BetType.Parlay, DisplayName = "Parlay", DisplayOrder = 5, IsActive = true, Description = "Multiple bets combined into one wager" },
                new BetTypeConfiguration { Id = 6, BetType = BetType.Future, DisplayName = "Future", DisplayOrder = 6, IsActive = true, Description = "Bet on long-term outcomes like championship winners" },
                new BetTypeConfiguration { Id = 7, BetType = BetType.Other, DisplayName = "Other", DisplayOrder = 7, IsActive = true, Description = "Other bet types" }
            );
        }
    }
}
