using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Attributes;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [DemoUserRestriction]
    public class SportLeaguesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SportLeaguesController(ApplicationDbContext context)
        {
            _context = context;
        }        // GET: SportLeagues
        public IActionResult Index()
        {
            // Redirect to the new BetSettings controller
            return RedirectToAction("Index", "BetSettings");
            
            /*
            var leagues = await _context.SportLeagues.ToListAsync();
            var betTypes = await _context.BetTypeConfigurations.OrderBy(b => b.DisplayOrder).ToListAsync();
            ViewBag.BetTypes = betTypes;
            return View(leagues);
            */
        }

        // GET: SportLeagues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportLeague = await _context.SportLeagues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sportLeague == null)
            {
                return NotFound();
            }

            return View(sportLeague);
        }

        // GET: SportLeagues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SportLeagues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] SportLeague sportLeague)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sportLeague);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sportLeague);
        }

        // GET: SportLeagues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportLeague = await _context.SportLeagues.FindAsync(id);
            if (sportLeague == null)
            {
                return NotFound();
            }
            return View(sportLeague);
        }

        // POST: SportLeagues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] SportLeague sportLeague)
        {
            if (id != sportLeague.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sportLeague);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SportLeagueExists(sportLeague.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sportLeague);
        }

        // GET: SportLeagues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportLeague = await _context.SportLeagues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sportLeague == null)
            {
                return NotFound();
            }

            return View(sportLeague);
        }

        // POST: SportLeagues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sportLeague = await _context.SportLeagues.FindAsync(id);
            if (sportLeague != null)
            {
                // Check if there are any bets using this sport league
                bool hasBets = await _context.Bets.AnyAsync(b => b.SportLeagueId == id);
                if (hasBets)
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete this sport/league because it has associated bets.");
                    return View("Delete", sportLeague);
                }

                _context.SportLeagues.Remove(sportLeague);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: SportLeagues/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var currentItem = await _context.SportLeagues.FindAsync(id);
            if (currentItem == null)
            {
                return NotFound();
            }

            var itemAbove = await _context.SportLeagues
                .Where(l => l.DisplayOrder < currentItem.DisplayOrder)
                .OrderByDescending(l => l.DisplayOrder)
                .FirstOrDefaultAsync();

            if (itemAbove != null)
            {
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemAbove.DisplayOrder;
                itemAbove.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemAbove);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SportLeagues/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var currentItem = await _context.SportLeagues.FindAsync(id);
            if (currentItem == null)
            {
                return NotFound();
            }

            var itemBelow = await _context.SportLeagues
                .Where(l => l.DisplayOrder > currentItem.DisplayOrder)
                .OrderBy(l => l.DisplayOrder)
                .FirstOrDefaultAsync();

            if (itemBelow != null)
            {
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemBelow.DisplayOrder;
                itemBelow.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemBelow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SportLeagues/DeleteBetType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBetType(int id)
        {
            var betType = await _context.BetTypeConfigurations.FindAsync(id);
            if (betType != null)
            {
                _context.BetTypeConfigurations.Remove(betType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: SportLeagues/MoveBetTypeUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveBetTypeUp(int id)
        {
            var currentItem = await _context.BetTypeConfigurations.FindAsync(id);
            if (currentItem == null)
            {
                return NotFound();
            }

            var itemAbove = await _context.BetTypeConfigurations
                .Where(b => b.DisplayOrder < currentItem.DisplayOrder)
                .OrderByDescending(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            if (itemAbove != null)
            {
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemAbove.DisplayOrder;
                itemAbove.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemAbove);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SportLeagues/MoveBetTypeDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveBetTypeDown(int id)
        {
            var currentItem = await _context.BetTypeConfigurations.FindAsync(id);
            if (currentItem == null)
            {
                return NotFound();
            }

            var itemBelow = await _context.BetTypeConfigurations
                .Where(b => b.DisplayOrder > currentItem.DisplayOrder)
                .OrderBy(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            if (itemBelow != null)
            {
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemBelow.DisplayOrder;
                itemBelow.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemBelow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SportLeagueExists(int id)
        {
            return _context.SportLeagues.Any(e => e.Id == id);
        }
    }
}
