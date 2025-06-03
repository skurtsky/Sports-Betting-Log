using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }        // GET: api/Bets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bet>>> GetBets()
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            // Filter bets by current user - only show bets belonging to this user
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id)
                .ToListAsync();
                
            return bets;
        }

        // GET: api/Bets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bet>> GetBet(int id)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            var bet = await _context.Bets
                .Include(b => b.SportLeague)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bet == null)
            {
                return NotFound();
            }
              // Check if user owns the bet - only allow access to their own bets
            bool canAccess = bet.UserId == currentUser.Id;
            if (!canAccess)
            {
                return Forbid();
            }

            return bet;
        }        // GET: api/Bets/Dashboard
        [HttpGet("Dashboard")]
        public async Task<ActionResult<object>> GetDashboardSummary()
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            // Filter bets by current user - only show bets belonging to this user
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == currentUser.Id)
                .ToListAsync();
                
            // Calculate dashboard summary stats
            var totalBets = bets.Count;
            var totalWins = bets.Count(b => b.Result == BetResult.WIN);
            var totalLosses = bets.Count(b => b.Result == BetResult.LOSS);
            var totalPushes = bets.Count(b => b.Result == BetResult.PUSH);
            var pendingBets = bets.Count(b => b.Result == BetResult.PENDING);
            
            var totalStake = bets.Sum(b => b.Stake);
            var netProfit = bets.Sum(b => b.AmountWonLost ?? 0);
            var roi = totalStake > 0 ? (netProfit / totalStake) * 100 : 0;
            var winPercentage = bets.Count(b => b.Result != BetResult.PENDING) > 0 
                ? (decimal)totalWins / bets.Count(b => b.Result != BetResult.PENDING) * 100 
                : 0;
                
            return new
            {
                TotalBets = totalBets,
                TotalWins = totalWins,
                TotalLosses = totalLosses,
                TotalPushes = totalPushes,
                PendingBets = pendingBets,
                TotalStake = totalStake,
                NetProfit = netProfit,
                ROI = roi,
                WinPercentage = winPercentage
            };
        }
    }
}
