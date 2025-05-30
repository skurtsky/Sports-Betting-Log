using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{
    public class BetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bets
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["SportSortParm"] = sortOrder == "Sport" ? "sport_desc" : "Sport";
            ViewData["ResultSortParm"] = sortOrder == "Result" ? "result_desc" : "Result";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var bets = from b in _context.Bets.Include(b => b.SportLeague)
                       select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                bets = bets.Where(b => 
                    b.Match.Contains(searchString) ||
                    b.BetSelection.Contains(searchString) ||
                    b.SportLeague.Name.Contains(searchString));
            }

            bets = sortOrder switch
            {
                "date_desc" => bets.OrderByDescending(b => b.BetDate),
                "Sport" => bets.OrderBy(b => b.SportLeague.Name),
                "sport_desc" => bets.OrderByDescending(b => b.SportLeague.Name),
                "Result" => bets.OrderBy(b => b.Result),
                "result_desc" => bets.OrderByDescending(b => b.Result),
                "Amount" => bets.OrderBy(b => b.AmountWonLost),
                "amount_desc" => bets.OrderByDescending(b => b.AmountWonLost),
                _ => bets.OrderBy(b => b.BetDate),
            };

            int pageSize = 10;
            return View(await PaginatedList<Bet>.CreateAsync(bets.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Bets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets
                .Include(b => b.SportLeague)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bet == null)
            {
                return NotFound();
            }

            return View(bet);
        }

        // GET: Bets/Create
        public IActionResult Create()
        {
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name");
            return View();
        }

        // POST: Bets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BetDate,SportLeagueId,Match,BetSelection,Stake,Odds,Result")] Bet bet)
        {
            if (ModelState.IsValid)
            {
                bet.CalculateWinLoss();
                _context.Add(bet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }

        // GET: Bets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }

        // POST: Bets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BetDate,SportLeagueId,Match,BetSelection,Stake,Odds,Result")] Bet bet)
        {
            if (id != bet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bet.CalculateWinLoss();
                    _context.Update(bet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BetExists(bet.Id))
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
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }

        // GET: Bets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets
                .Include(b => b.SportLeague)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bet == null)
            {
                return NotFound();
            }

            return View(bet);
        }

        // POST: Bets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bet = await _context.Bets.FindAsync(id);
            if (bet != null)
            {
                _context.Bets.Remove(bet);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool BetExists(int id)
        {
            return _context.Bets.Any(e => e.Id == id);
        }
    }
}
