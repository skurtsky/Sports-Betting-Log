using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SportsBettingTracker.Services
{
    public class DemoUserCleanupService : BackgroundService
    {
        private readonly ILogger<DemoUserCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _demoUserLifetime = TimeSpan.FromHours(24); // Demo users live for 24 hours
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);  // Run cleanup every hour

        public DemoUserCleanupService(
            ILogger<DemoUserCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Demo User Cleanup Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupDemoUsersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up demo users.");
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupDemoUsersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Find demo users older than the allowed lifetime
            var cutoffDate = DateTime.UtcNow.Subtract(_demoUserLifetime);
            var demoUsersToDelete = await userManager.Users
                .Where(u => u.IsDemoUser && u.CreatedAt < cutoffDate)
                .ToListAsync();

            if (!demoUsersToDelete.Any())
            {
                return;
            }

            _logger.LogInformation("Found {Count} expired demo users to remove", demoUsersToDelete.Count);

            foreach (var user in demoUsersToDelete)
            {
                // Delete all associated bets
                var userBets = await dbContext.Bets
                    .Where(b => b.UserId == user.Id)
                    .ToListAsync();

                if (userBets.Any())
                {
                    _logger.LogInformation("Removing {Count} bets for demo user {UserId}", 
                        userBets.Count, user.Id);
                    dbContext.Bets.RemoveRange(userBets);
                }

                // Delete the user
                await userManager.DeleteAsync(user);
                _logger.LogInformation("Removed expired demo user {UserId}", user.Id);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
