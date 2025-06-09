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
    public class FeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Feed
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Get IDs of users that the current user follows
            var followedUserIds = await _context.UserFollows
                .Where(uf => uf.FollowerId == currentUser.Id)
                .Select(uf => uf.FollowingId)
                .ToListAsync();            // Get public bets from followed users and the current user's own public bets
            var betsQuery = _context.Bets
                .Include(b => b.User)
                .Include(b => b.SportLeague)
                .Include(b => b.Likes)
                .Include(b => b.Comments)
                .Where(b => (followedUserIds.Contains(b.UserId) || b.UserId == currentUser.Id) && b.IsPublic)
                .OrderByDescending(b => b.BetDate);

            int pageSize = 10;
            var bets = await PaginatedList<Bet>.CreateAsync(betsQuery, pageNumber ?? 1, pageSize);

            return View(bets);
        }

        // POST: Feed/Like/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id, bool isLike)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }

            // Check if user has already liked/disliked this bet
            var existingLike = await _context.BetLikes
                .FirstOrDefaultAsync(bl => bl.BetId == id && bl.UserId == currentUser.Id);

            if (existingLike != null)
            {
                if (existingLike.IsLike == isLike)
                {
                    // Remove the like/dislike if clicking the same button again
                    _context.BetLikes.Remove(existingLike);
                }
                else
                {
                    // Change from like to dislike or vice versa
                    existingLike.IsLike = isLike;
                }
            }
            else
            {
                // Create new like/dislike
                var betLike = new BetLike
                {
                    BetId = id,
                    UserId = currentUser.Id,
                    IsLike = isLike
                };
                _context.BetLikes.Add(betLike);
            }

            await _context.SaveChangesAsync();

            // Return updated like/dislike counts
            var likeCount = await _context.BetLikes.CountAsync(bl => bl.BetId == id && bl.IsLike);
            var dislikeCount = await _context.BetLikes.CountAsync(bl => bl.BetId == id && !bl.IsLike);

            return Json(new { success = true, likeCount, dislikeCount });
        }

        // POST: Feed/Comment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment cannot be empty");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }

            var comment = new BetComment
            {
                BetId = id,
                UserId = currentUser.Id,
                Content = content
            };

            _context.BetComments.Add(comment);
            await _context.SaveChangesAsync();

            // Load the user for the comment to include in response
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();            return Json(new
            {
                success = true,
                comment = new
                {
                    id = comment.Id,
                    content = comment.Content,
                    userName = comment.User?.DisplayName ?? "Unknown User",
                    createdAt = comment.CreatedAt
                }
            });
        }
    }
}
