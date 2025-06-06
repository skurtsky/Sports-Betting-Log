using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }        // GET: Profile/UserProfile/{id}
        public async Task<IActionResult> UserProfile(string id)
        {
            var currentUser = await _userManager.GetUserAsync(base.User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var profileUser = await _userManager.FindByIdAsync(id);
            if (profileUser == null)
            {
                return NotFound();
            }

            // Check if profile is private and user is not viewing their own profile
            if (!profileUser.IsProfilePublic && currentUser.Id != profileUser.Id)
            {
                return View("PrivateProfile");
            }

            // Get user's public bets
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == id && (b.IsPublic || currentUser.Id == id))
                .OrderByDescending(b => b.BetDate)
                .Take(10)
                .ToListAsync();

            // Calculate stats
            var allBets = await _context.Bets
                .Where(b => b.UserId == id && (b.IsPublic || currentUser.Id == id))
                .ToListAsync();

            var totalBets = allBets.Count;
            var winningBets = allBets.Count(b => b.Result == BetResult.WIN);
            var totalStake = allBets.Sum(b => b.Stake);
            var netProfit = allBets.Sum(b => b.AmountWonLost ?? 0);            var winRate = totalBets > 0 ? (double)winningBets / totalBets * 100.0 : 0.0;
            var roi = totalStake > 0 ? (double)(netProfit / totalStake * 100) : 0.0;

            // Check if current user follows this profile
            var isFollowing = await _context.UserFollows
                .AnyAsync(uf => uf.FollowerId == currentUser.Id && uf.FollowingId == id);

            var viewModel = new ProfileViewModel
            {
                User = profileUser,
                RecentBets = bets,
                TotalBets = totalBets,
                WinRate = winRate,
                NetProfit = netProfit,
                ROI = roi,
                IsFollowing = isFollowing,
                IsOwnProfile = currentUser.Id == id
            };

            return View(viewModel);
        }

        // GET: Profile/AllBets/{id}
        public async Task<IActionResult> AllBets(string id, int? pageNumber)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var profileUser = await _userManager.FindByIdAsync(id);
            if (profileUser == null)
            {
                return NotFound();
            }

            // Check if profile is private and user is not viewing their own profile
            if (!profileUser.IsProfilePublic && currentUser.Id != profileUser.Id)
            {
                return View("PrivateProfile");
            }

            var betsQuery = _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == id && (b.IsPublic || currentUser.Id == id))
                .OrderByDescending(b => b.BetDate);

            int pageSize = 20;
            var bets = await PaginatedList<Bet>.CreateAsync(betsQuery, pageNumber ?? 1, pageSize);

            ViewData["ProfileUser"] = profileUser;
            return View(bets);
        }

        // POST: Profile/Follow/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (currentUser.Id == id)
            {
                return BadRequest("Cannot follow yourself");
            }

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                return NotFound();
            }

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == currentUser.Id && uf.FollowingId == id);

            if (existingFollow == null)
            {
                var userFollow = new UserFollow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = id
                };
                _context.UserFollows.Add(userFollow);
            }
            else
            {
                _context.UserFollows.Remove(existingFollow);
            }

            await _context.SaveChangesAsync();

            // Return updated follower count
            var followerCount = await _context.UserFollows.CountAsync(uf => uf.FollowingId == id);
            return Json(new { success = true, isFollowing = existingFollow == null, followerCount });
        }

        // POST: Profile/UpdatePrivacy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivacy(bool isProfilePublic, bool defaultBetPrivacy)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            currentUser.IsProfilePublic = isProfilePublic;
            currentUser.DefaultBetPrivacy = defaultBetPrivacy;

            await _userManager.UpdateAsync(currentUser);

            return RedirectToAction(nameof(UserProfile), new { id = currentUser.Id });
        }
    }
}
