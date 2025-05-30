using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index(string dateRange)
        {
            // Default to YTD if no date range is specified
            dateRange ??= "YTD";

            DateTime startDate = GetStartDateFromRange(dateRange);
            DateTime endDate = DateTime.Today;

            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.BetDate >= startDate && b.BetDate <= endDate)
                .ToListAsync();            // Calculate net profit by sport/league
            var profitBySport = bets
                .Where(b => b.AmountWonLost.HasValue) // Only include bets with results
                .GroupBy(b => b.SportLeague.Name)
                .Select(g => new ProfitBySport
                {
                    SportLeagueName = g.Key,
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    NetProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    WinPercentage = g.Count() > 0 ? (decimal)g.Count(b => b.Result == BetResult.WIN) / g.Count() * 100 : 0
                })
                .OrderByDescending(p => p.NetProfit)
                .ToList();
                
            // Calculate net profit by bet type
            var profitByBetType = bets
                .Where(b => b.AmountWonLost.HasValue) // Only include bets with results
                .GroupBy(b => b.BetType)
                .Select(g => new ProfitByBetType
                {
                    BetType = g.Key,
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    NetProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    WinPercentage = g.Count() > 0 ? (decimal)g.Count(b => b.Result == BetResult.WIN) / g.Count() * 100 : 0
                })
                .OrderByDescending(p => p.NetProfit)
                .ToList();

            // Prepare data for profit chart
            var chartData = bets
                .Where(b => b.AmountWonLost.HasValue) // Only include bets with results
                .GroupBy(b => b.BetDate.ToString("MM/dd/yyyy"))
                .OrderBy(g => DateTime.Parse(g.Key))
                .Select(g => new
                {
                    Date = g.Key,
                    Profit = g.Sum(b => b.AmountWonLost ?? 0)
                })
                .ToList();            // Calculate streak data
            var completedBets = bets
                .Where(b => b.Result != BetResult.PENDING)
                .OrderBy(b => b.BetDate)
                .ToList();
            
            var (currentStreak, streakType) = CalculateCurrentStreak(completedBets);
            var (longestWinStreak, longestLossStreak) = CalculateLongestStreaks(completedBets);
            
            // Calculate total stake for ROI
            decimal totalStake = bets.Sum(b => b.Stake);
            decimal netProfit = bets.Sum(b => b.AmountWonLost ?? 0);
            decimal roi = totalStake > 0 ? (netProfit / totalStake) * 100 : 0;

            // Calculate cumulative profit data
            var cumulativeProfit = CalculateCumulativeProfit(chartData
                .Select(d => d.Profit)
                .ToList());
                
            var viewModel = new DashboardViewModel
            {
                DateRange = dateRange,
                StartDate = startDate,
                EndDate = endDate,
                TotalBets = bets.Count,
                TotalWins = bets.Count(b => b.Result == BetResult.WIN),
                TotalLosses = bets.Count(b => b.Result == BetResult.LOSS),
                TotalPushes = bets.Count(b => b.Result == BetResult.PUSH),
                PendingBets = bets.Count(b => b.Result == BetResult.PENDING),
                TotalStake = totalStake,
                NetProfit = netProfit,
                ROI = roi,
                WinPercentage = bets.Count(b => b.Result != BetResult.PENDING) > 0 
                    ? (decimal)bets.Count(b => b.Result == BetResult.WIN) / bets.Count(b => b.Result != BetResult.PENDING) * 100 
                    : 0,
                CurrentStreak = currentStreak,
                StreakType = streakType,                LongestWinStreak = longestWinStreak,
                LongestLossStreak = longestLossStreak,
                ProfitBySport = profitBySport,
                ProfitByBetType = profitByBetType,
                ChartLabels = chartData.Select(d => d.Date).ToList(),
                ChartData = chartData.Select(d => d.Profit).ToList(),
                CumulativeChartData = cumulativeProfit
            };

            return View(viewModel);
        }        private DateTime GetStartDateFromRange(string dateRange)
        {
            return dateRange switch
            {
                "Last7Days" => DateTime.Today.AddDays(-7),
                "Last30Days" => DateTime.Today.AddDays(-30),
                "Last90Days" => DateTime.Today.AddDays(-90),
                "Last6Months" => DateTime.Today.AddMonths(-6),
                "YTD" => new DateTime(DateTime.Today.Year, 1, 1),
                "AllTime" => DateTime.MinValue,
                _ => new DateTime(DateTime.Today.Year, 1, 1), // Default to YTD
            };
        }
        
        private (int streakCount, string streakType) CalculateCurrentStreak(List<Bet> completedBets)
        {
            if (!completedBets.Any())
                return (0, string.Empty);
                
            var reversedBets = completedBets.OrderByDescending(b => b.BetDate).ToList();
            var firstBet = reversedBets.First();
            
            // Current streak type is based on most recent bet
            string streakType = firstBet.Result == BetResult.WIN ? "wins" : "losses";
            
            // Count consecutive bets with the same result
            int streakCount = 0;
            foreach (var bet in reversedBets)
            {
                if ((streakType == "wins" && bet.Result == BetResult.WIN) || 
                    (streakType == "losses" && bet.Result == BetResult.LOSS))
                {
                    streakCount++;
                }
                else
                {
                    break;
                }
            }
            
            return (streakCount, streakType);
        }
        
        private (int longestWinStreak, int longestLossStreak) CalculateLongestStreaks(List<Bet> completedBets)
        {
            if (!completedBets.Any())
                return (0, 0);
                
            int longestWinStreak = 0;
            int longestLossStreak = 0;
            int currentWinStreak = 0;
            int currentLossStreak = 0;
            
            foreach (var bet in completedBets)
            {
                if (bet.Result == BetResult.WIN)
                {
                    currentWinStreak++;
                    currentLossStreak = 0;
                    longestWinStreak = Math.Max(longestWinStreak, currentWinStreak);
                }
                else if (bet.Result == BetResult.LOSS)
                {
                    currentLossStreak++;
                    currentWinStreak = 0;
                    longestLossStreak = Math.Max(longestLossStreak, currentLossStreak);
                }
                else
                {
                    // PUSH - reset both streaks
                    currentWinStreak = 0;
                    currentLossStreak = 0;
                }
            }
            
            return (longestWinStreak, longestLossStreak);
        }
        
        private List<decimal> CalculateCumulativeProfit(List<decimal> dailyProfit)
        {
            if (!dailyProfit.Any())
                return new List<decimal>();
                
            List<decimal> cumulativeProfit = new List<decimal>();
            decimal runningSum = 0;
            
            foreach (var profit in dailyProfit)
            {
                runningSum += profit;
                cumulativeProfit.Add(runningSum);
            }
            
            return cumulativeProfit;
        }
    }
}
