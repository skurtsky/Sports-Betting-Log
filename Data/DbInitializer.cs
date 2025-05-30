using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Models;
using System;
using System.Linq;

namespace SportsBettingTracker.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Make sure the database is created
            context.Database.EnsureCreated();
            
            // Check if there are any sport leagues (to avoid adding data multiple times)
            if (context.SportLeagues.Any())
            {
                return; // DB has been seeded
            }

            // Add some sample sport leagues
            var leagues = new SportLeague[]
            {
                new SportLeague { Name = "NFL", Description = "National Football League" },
                new SportLeague { Name = "NBA", Description = "National Basketball Association" },
                new SportLeague { Name = "MLB", Description = "Major League Baseball" },
                new SportLeague { Name = "NHL", Description = "National Hockey League" },
                new SportLeague { Name = "UFC", Description = "Ultimate Fighting Championship" }
            };

            context.SportLeagues.AddRange(leagues);
            context.SaveChanges();

            // Log success message
            Console.WriteLine("Database has been seeded with initial data.");
        }
    }
}
