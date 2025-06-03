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
    public class CalendarController : Controller
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
        
        // Helper method to convert Bet to CalendarBetViewModel
        private CalendarBetViewModel ConvertToCalendarBetViewModel(Bet bet)
        {
            return new CalendarBetViewModel
            {
                Id = bet.Id,
                Match = bet.Match,
                BetSelection = bet.BetSelection,
                Result = bet.Result,
                SportLeagueName = bet.SportLeague?.Name ?? "Unknown",
                Stake = bet.Stake,
                AmountWonLost = bet.AmountWonLost
            };
        }

        // GET: Calendar
        public async Task<IActionResult> Index(int? year, int? month)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Determine which month to display
            DateTime currentMonth;
            
            if (year.HasValue && month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                currentMonth = new DateTime(year.Value, month.Value, 1);
            }
            else
            {
                currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            // Calculate first and last day of the month
            DateTime firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get all bets for the current month
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id && 
                            b.BetDate >= firstDayOfMonth && 
                            b.BetDate <= lastDayOfMonth)
                .OrderBy(b => b.BetDate)
                .ToListAsync();

            // Group bets by date
            var betsByDate = new Dictionary<DateTime, List<CalendarBetViewModel>>();
            
            // Pre-populate all days of the month in dictionary
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var date = new DateTime(currentMonth.Year, currentMonth.Month, day);
                betsByDate[date] = new List<CalendarBetViewModel>();
            }            // Add bets to dictionary
            foreach (var bet in bets)
            {
                var date = bet.BetDate.Date;
                var calendarBet = ConvertToCalendarBetViewModel(bet);

                if (!betsByDate.ContainsKey(date))
                {
                    betsByDate[date] = new List<CalendarBetViewModel>();
                }
                
                betsByDate[date].Add(calendarBet);
            }

            // Calculate monthly stats
            decimal totalMonthProfit = bets.Sum(b => b.AmountWonLost ?? 0);
            int totalMonthBets = bets.Count;
            
            // Calculate winning and losing days
            int winningDays = 0;
            int losingDays = 0;
            
            foreach (var dateGroup in betsByDate)
            {
                if (dateGroup.Value.Count > 0)
                {
                    decimal dailyProfit = dateGroup.Value.Sum(b => b.AmountWonLost ?? 0);
                    
                    if (dailyProfit > 0)
                    {
                        winningDays++;
                    }
                    else if (dailyProfit < 0)
                    {
                        losingDays++;
                    }
                }
            }

            // Create the view model
            var viewModel = new CalendarViewModel
            {
                CurrentMonth = currentMonth,
                BetsByDate = betsByDate,
                TotalMonthProfit = totalMonthProfit,
                TotalMonthBets = totalMonthBets,
                WinningDays = winningDays,
                LosingDays = losingDays
            };            return View(viewModel);
        }
        
        // GET: Calendar/DailyDetails
        public async Task<IActionResult> DailyDetails(int year, int month, int day)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            // Validate date
            DateTime requestedDate;
            try
            {
                requestedDate = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return RedirectToAction(nameof(Index));
            }
              // Get all bets for the requested date
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id && 
                            b.BetDate.Date == requestedDate.Date)
                .ToListAsync();
            
            // Convert to view models and order by amount won/lost
            var betViewModels = bets
                .Select(b => ConvertToCalendarBetViewModel(b))
                .OrderByDescending(b => b.AmountWonLost)
                .ToList();
            
            // Pass the date to the view
            ViewBag.Date = requestedDate;
            
            return View(betViewModels);
        }
    }
}
