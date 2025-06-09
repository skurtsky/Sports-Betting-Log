using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    [Authorize]
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaderboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leaderboard
        public async Task<IActionResult> Index(string dateRange = "ytd")
        {
            // Default to YTD if no date range is specified
            dateRange = dateRange?.ToLower() ?? "ytd";

            DateTime startDate = GetStartDateFromRange(dateRange);
            DateTime endDate = DateTime.Today;

            // Get users with public profiles who have bets
            var usersWithPublicProfiles = await _userManager.Users
                .Where(u => u.IsProfilePublic)
                .ToListAsync();

            var leaderboardEntries = new List<LeaderboardViewModel>();

            foreach (var user in usersWithPublicProfiles)
            {
                // Get user's public bets within the specified date range
                var userBets = await _context.Bets
                    .Where(b => b.UserId == user.Id && b.IsPublic && b.BetDate >= startDate && b.BetDate <= endDate)
                    .ToListAsync();

                if (userBets.Count > 0) // Only include users who have bets in the specified time range
                {
                    var totalBets = userBets.Count;
                    var winningBets = userBets.Count(b => b.Result == BetResult.WIN);
                    var totalStake = userBets.Sum(b => b.Stake);
                    var netProfit = userBets.Sum(b => b.AmountWonLost ?? 0);
                    var winRate = totalBets > 0 ? (double)winningBets / totalBets * 100.0 : 0.0;
                    var roi = totalStake > 0 ? (double)(netProfit / totalStake * 100) : 0.0;

                    // Get followers count
                    var followersCount = await _context.UserFollows
                        .CountAsync(uf => uf.FollowingId == user.Id);

                    // Add to leaderboard entries
                    leaderboardEntries.Add(new LeaderboardViewModel
                    {
                        User = user,
                        TotalBets = totalBets,
                        WinRate = winRate,
                        NetProfit = netProfit,
                        ROI = roi,
                        FollowersCount = followersCount
                    });
                }
            }

            // Order by ROI descending
            var orderedLeaderboard = leaderboardEntries.OrderByDescending(e => e.ROI).ToList();

            // Pass date range to view
            ViewData["DateRange"] = dateRange;
            ViewData["StartDate"] = startDate.ToString("MMM d, yyyy");
            ViewData["EndDate"] = endDate.ToString("MMM d, yyyy");

            return View(orderedLeaderboard);
        }

        // GET: Leaderboard/Search
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            {
                // Return top 5 most-followed users with public profiles
                var topUsers = await _userManager.Users
                    .Where(u => u.IsProfilePublic)
                    .OrderByDescending(u => _context.UserFollows.Count(uf => uf.FollowingId == u.Id))
                    .Take(5)
                    .Select(u => new
                    {
                        id = u.Id,
                        displayName = u.DisplayName,
                        followerCount = _context.UserFollows.Count(uf => uf.FollowingId == u.Id)
                    })
                    .ToListAsync();
                return Json(topUsers);
            }

            query = query.ToLower();

            // Search for users with public profiles where display name contains query (case-insensitive)
            var matchingUsers = await _userManager.Users
                .Where(u => u.IsProfilePublic && u.DisplayName.ToLower().Contains(query))
                .OrderByDescending(u => _context.UserFollows.Count(uf => uf.FollowingId == u.Id)) // Order by followers count
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    displayName = u.DisplayName,
                    followerCount = _context.UserFollows.Count(uf => uf.FollowingId == u.Id)
                })
                .ToListAsync();

            return Json(matchingUsers);
        }

        // Helper method to get date range
        private DateTime GetStartDateFromRange(string dateRange)
        {
            DateTime today = DateTime.Today;
            
            return dateRange switch
            {
                "7days" => today.AddDays(-7),
                "30days" => today.AddDays(-30),
                "3months" => today.AddMonths(-3),
                "6months" => today.AddMonths(-6),
                "12months" => today.AddYears(-1),
                "ytd" or _ => new DateTime(today.Year, 1, 1) // Year to date (default)
            };
        }
    }
}
