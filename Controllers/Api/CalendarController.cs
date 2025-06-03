using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Calendar/MonthSummary
        [HttpGet("MonthSummary")]
        public async Task<ActionResult<object>> GetMonthSummary(int year, int month)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id && 
                          b.BetDate >= firstDayOfMonth && 
                          b.BetDate <= lastDayOfMonth)
                .ToListAsync();

            // Calculate monthly stats
            var totalMonthBets = bets.Count;
            var totalMonthProfit = bets.Sum(b => b.AmountWonLost ?? 0);

            // Calculate winning and losing days
            var dailyResults = bets
                .GroupBy(b => b.BetDate.Date)
                .Select(g => new { 
                    Date = g.Key,
                    DailyProfit = g.Sum(b => b.AmountWonLost ?? 0)
                })
                .ToList();

            var winningDays = dailyResults.Count(d => d.DailyProfit > 0);
            var losingDays = dailyResults.Count(d => d.DailyProfit < 0);

            return new
            {
                TotalBets = totalMonthBets,
                TotalProfit = totalMonthProfit,
                WinningDays = winningDays,
                LosingDays = losingDays,
                FormattedProfit = totalMonthProfit.ToString("C")
            };
        }

        // GET: api/Calendar/DayBets
        [HttpGet("DayBets")]
        public async Task<ActionResult<object>> GetDayBets(int year, int month, int day)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var requestedDate = new DateTime(year, month, day);

            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id && 
                          b.BetDate.Date == requestedDate.Date)
                .OrderByDescending(b => b.AmountWonLost)
                .Select(b => new CalendarBetViewModel
                {
                    Id = b.Id,
                    Match = b.Match,
                    BetSelection = b.BetSelection,
                    Result = b.Result,
                    SportLeagueName = b.SportLeague.Name,
                    Stake = b.Stake,
                    AmountWonLost = b.AmountWonLost
                })
                .ToListAsync();

            return new { Bets = bets };
        }
    }
}
