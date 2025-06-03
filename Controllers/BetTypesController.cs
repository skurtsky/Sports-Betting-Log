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
    public class BetTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BetTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BetTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.BetTypeConfigurations.OrderBy(b => b.DisplayOrder).ToListAsync());
        }

        // GET: BetTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var betTypeConfig = await _context.BetTypeConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (betTypeConfig == null)
            {
                return NotFound();
            }

            return View(betTypeConfig);
        }

        // GET: BetTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var betTypeConfig = await _context.BetTypeConfigurations.FindAsync(id);
            if (betTypeConfig == null)
            {
                return NotFound();
            }
            return View(betTypeConfig);
        }

        // POST: BetTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BetType,DisplayName,DisplayOrder,IsActive,Description")] BetTypeConfiguration betTypeConfig)
        {
            if (id != betTypeConfig.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(betTypeConfig);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BetTypeConfigExists(betTypeConfig.Id))
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
            return View(betTypeConfig);
        }
        
        // POST: BetTypes/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
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
                // Swap display orders
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemAbove.DisplayOrder;
                itemAbove.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemAbove);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // POST: BetTypes/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
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
                // Swap display orders
                int tempOrder = currentItem.DisplayOrder;
                currentItem.DisplayOrder = itemBelow.DisplayOrder;
                itemBelow.DisplayOrder = tempOrder;

                _context.Update(currentItem);
                _context.Update(itemBelow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: BetTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BetTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BetType,DisplayName,DisplayOrder,IsActive,Description")] BetTypeConfiguration betTypeConfig)
        {
            if (ModelState.IsValid)
            {
                _context.Add(betTypeConfig);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "SportLeagues");
            }
            return View(betTypeConfig);
        }

        // POST: BetTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var betType = await _context.BetTypeConfigurations.FindAsync(id);
            if (betType != null)
            {
                _context.BetTypeConfigurations.Remove(betType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "SportLeagues");
        }

        private bool BetTypeConfigExists(int id)
        {
            return _context.BetTypeConfigurations.Any(e => e.Id == id);
        }
    }
}
