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
                .ToListAsync();

            // Calculate net profit by sport/league
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
                .ToList();

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
                NetProfit = bets.Sum(b => b.AmountWonLost ?? 0),
                WinPercentage = bets.Count(b => b.Result != BetResult.PENDING) > 0 
                    ? (decimal)bets.Count(b => b.Result == BetResult.WIN) / bets.Count(b => b.Result != BetResult.PENDING) * 100 
                    : 0,
                ProfitBySport = profitBySport,
                ChartLabels = chartData.Select(d => d.Date).ToList(),
                ChartData = chartData.Select(d => d.Profit).ToList(),
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
    }
}
