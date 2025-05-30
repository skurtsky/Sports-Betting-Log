using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    public class SportLeaguesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SportLeaguesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SportLeagues
        public async Task<IActionResult> Index()
        {
            return View(await _context.SportLeagues.ToListAsync());
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

        private bool SportLeagueExists(int id)
        {
            return _context.SportLeagues.Any(e => e.Id == id);
        }
    }
}
