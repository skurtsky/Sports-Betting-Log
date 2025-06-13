using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.Services;
using SportsBettingTracker.ViewModels;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{    
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class DashboardController : Controller
    {        
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly SportsBettingTracker.Services.BetRecommendationService _recommendationService;

        public DashboardController(
            ApplicationDbContext context, 
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            SportsBettingTracker.Services.BetRecommendationService recommendationService)
        {
            _context = context;
            _userManager = userManager;
            _recommendationService = recommendationService;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index(string dateRange)
        {
            // Default to YTD if no date range is specified
            dateRange ??= "YTD";
            
            DateTime startDate = GetStartDateFromRange(dateRange);
            DateTime endDate = DateTime.Today;
            
            // Get SportLeagues for the pending bets widget dropdowns
            ViewBag.SportLeagues = await _context.SportLeagues.OrderBy(sl => sl.Name).ToListAsync();

            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;

            // Base query for regular bets (respects date range)
            var betsQuery = _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.BetDate >= startDate && b.BetDate <= endDate);
                
            // Special query for future bets (includes all dates)
            var futureBetsQuery = _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.BetType == BetType.Future && b.Result == BetResult.PENDING);
                
            // Filter by user - always restrict to current user''s bets
            if (userId != null)
            {
                betsQuery = betsQuery.Where(b => b.UserId == userId);
                futureBetsQuery = futureBetsQuery.Where(b => b.UserId == userId);
            }
                
            var bets = await betsQuery.ToListAsync();
            var futureBets = await futureBetsQuery.OrderBy(b => b.BetDate).ToListAsync();

            // Calculate net profit by sport/league
            var profitBySport = bets
                .Where(b => b.AmountWonLost.HasValue && b.SportLeague != null)
                .GroupBy(b => b.SportLeague!.Name)
                .Select(g => new ProfitBySport
                {
                    SportLeagueName = g.Key,
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    NetProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    WinPercentage = g.Count() > 0 ? (decimal)g.Count(b => b.Result == BetResult.WIN) / g.Count() * 100 : 0,
                    MedianBet = g.Count() > 0 ? CalculateMedian(g.Select(b => b.Stake)) : 0,
                    MedianOdds = g.Count() > 0 ? CalculateMedianDecimalOdds(g.Select(b => b.Odds)) : 0
                })
                .OrderByDescending(p => p.NetProfit)
                .ToList();
                
            // Calculate net profit by bet type
            var profitByBetType = bets
                .Where(b => b.AmountWonLost.HasValue)
                .GroupBy(b => b.BetType)
                .Select(g => new ProfitByBetType
                {
                    BetType = g.Key,
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    NetProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    WinPercentage = g.Count() > 0 ? (decimal)g.Count(b => b.Result == BetResult.WIN) / g.Count() * 100 : 0,
                    MedianBet = g.Count() > 0 ? CalculateMedian(g.Select(b => b.Stake)) : 0,
                    MedianOdds = g.Count() > 0 ? CalculateMedianDecimalOdds(g.Select(b => b.Odds)) : 0
                })
                .OrderByDescending(p => p.NetProfit)
                .ToList();

            // Prepare data for profit chart
            var chartData = bets
                .Where(b => b.AmountWonLost.HasValue)
                .GroupBy(b => b.BetDate.ToString("MM/dd/yyyy"))
                .OrderBy(g => DateTime.Parse(g.Key))
                .Select(g => new
                {
                    Date = g.Key,
                    Profit = g.Sum(b => b.AmountWonLost ?? 0)
                })
                .ToList();

            // Calculate streak data
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
                
            // Get recommendations if the user has enough bet history
            List<BetRecommendationViewModel> recommendations = new List<BetRecommendationViewModel>();
            if (userId != null && bets.Count >= 10)
            {
                var serviceRecommendations = await _recommendationService.GetRecommendationsAsync(userId, 2);
                recommendations = serviceRecommendations.Select(r => new BetRecommendationViewModel
                {
                    Type = (RecommendationTypeViewModel)r.Type,
                    SportLeagueId = r.SportLeagueId,
                    SportLeagueName = r.SportLeagueName,
                    BetTypeName = r.BetTypeName,
                    TeamName = r.TeamName,
                    Confidence = r.Confidence,
                    Reasoning = r.Reasoning,
                    SuggestedAction = r.SuggestedAction
                }).ToList();
            }
            
            var viewModel = new DashboardViewModel
            {
                DateRange = dateRange,
                StartDate = startDate,
                EndDate = endDate,
                TotalBets = bets.Count,
                TotalWins = bets.Count(b => b.Result == BetResult.WIN),
                TotalLosses = bets.Count(b => b.Result == BetResult.LOSS),
                TotalPushes = bets.Count(b => b.Result == BetResult.PUSH),
                PendingBets = bets.Where(b => b.Result == BetResult.PENDING && b.BetType != BetType.Future).ToList(),
                FutureBets = futureBets, // Use the separately queried future bets
                TotalStake = totalStake,
                NetProfit = netProfit,
                ROI = roi,
                WinPercentage = bets.Count(b => b.Result != BetResult.PENDING) > 0 
                    ? (decimal)bets.Count(b => b.Result == BetResult.WIN) / bets.Count(b => b.Result != BetResult.PENDING) * 100 
                    : 0,
                CurrentStreak = currentStreak,
                StreakType = streakType,
                LongestWinStreak = longestWinStreak,
                LongestLossStreak = longestLossStreak,
                ProfitBySport = profitBySport,
                ProfitByBetType = profitByBetType,
                ChartLabels = chartData.Select(d => d.Date).ToList(),
                ChartData = chartData.Select(d => d.Profit).ToList(),
                CumulativeChartData = cumulativeProfit,
                TopRecommendations = recommendations
            };

            return View(viewModel);
        }
        
        private DateTime GetStartDateFromRange(string dateRange)
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
        
        /// <summary>
        /// Converts American odds to decimal odds format
        /// </summary>
        /// <param name="americanOdds">American odds value</param>
        /// <returns>Decimal odds equivalent</returns>
        private decimal ConvertAmericanToDecimalOdds(int americanOdds)
        {
            if (americanOdds > 0)
                return (americanOdds / 100m) + 1m;
            else if (americanOdds < 0)
                return (100m / Math.Abs((decimal)americanOdds)) + 1m;
            else
                return 1m; // odds of 0 is effectively 1.0 in decimal (even money)
        }
        
        /// <summary>
        /// Calculates the median value from a collection of decimal values
        /// </summary>
        /// <param name="values">Collection of decimal values</param>
        /// <returns>Median value</returns>
        private decimal CalculateMedian(IEnumerable<decimal> values)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            int count = sortedValues.Count;
            
            if (count == 0)
                return 0;
                
            if (count % 2 == 0)
            {
                // Even count - average the two middle values
                int middle = count / 2;
                return (sortedValues[middle - 1] + sortedValues[middle]) / 2m;
            }
            else
            {
                // Odd count - return the middle value
                return sortedValues[count / 2];
            }
        }
        
        /// <summary>
        /// Calculates median decimal odds from a collection of American odds
        /// </summary>
        /// <param name="americanOdds">Collection of American odds values</param>
        /// <returns>Median odds in decimal format</returns>
        private decimal CalculateMedianDecimalOdds(IEnumerable<int> americanOdds)
        {
            if (!americanOdds.Any())
                return 0;
                
            // Convert each American odds to decimal odds and then find median
            var decimalOdds = americanOdds.Select(odds => ConvertAmericanToDecimalOdds(odds));
            return CalculateMedian(decimalOdds);
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