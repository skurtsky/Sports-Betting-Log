using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Attributes;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SportsBettingTracker.Controllers
{    [Microsoft.AspNetCore.Authorization.Authorize]
    [DemoUserRestriction]
    [Route("[controller]")]
    public class BetSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BetSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BetSettings
        [HttpGet]
        [Route("")]
        [Route("BetSettings")]
        [Route("SportLeagues")]  // Legacy route support
        public async Task<IActionResult> Index()
        {
            var leagues = await _context.SportLeagues.OrderBy(l => l.DisplayOrder).ToListAsync();
            var betTypes = await _context.BetTypeConfigurations.OrderBy(b => b.DisplayOrder).ToListAsync();
            ViewBag.BetTypes = betTypes;
            return View(leagues);
        }

        #region Sport League Actions
          // GET: BetSettings/CreateLeague
        [HttpGet]
        [Route("CreateLeague")]
        public IActionResult CreateLeague()
        {
            return View();
        }

        // POST: BetSettings/CreateLeague
        [HttpPost]
        [Route("CreateLeague")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeague([Bind("Id,Name,IsActive")] SportLeague sportLeague)
        {
            if (ModelState.IsValid)
            {
                // Set default display order to end of list
                if (await _context.SportLeagues.AnyAsync())
                {
                    sportLeague.DisplayOrder = await _context.SportLeagues.MaxAsync(sl => sl.DisplayOrder) + 1;
                }
                else
                {
                    sportLeague.DisplayOrder = 1;
                }
                
                _context.Add(sportLeague);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sportLeague);
        }

        // GET: BetSettings/EditLeague/5
        [HttpGet]
        [Route("EditLeague/{id}")]
        public async Task<IActionResult> EditLeague(int? id)
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

        // POST: BetSettings/EditLeague/5
        [HttpPost]
        [Route("EditLeague/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeague(int id, [Bind("Id,Name,DisplayOrder,IsActive")] SportLeague sportLeague)
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

        // GET: BetSettings/DeleteLeague/5
        [HttpGet]
        [Route("DeleteLeague/{id}")]
        public async Task<IActionResult> DeleteLeague(int? id)
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

        // POST: BetSettings/DeleteLeague/5
        [HttpPost]
        [Route("DeleteLeague/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeagueConfirmed(int id)
        {
            var sportLeague = await _context.SportLeagues.FindAsync(id);
            if (sportLeague != null)
            {
                _context.SportLeagues.Remove(sportLeague);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: BetSettings/MoveUpLeague/5
        [HttpPost]
        [Route("MoveUpLeague/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUpLeague(int id)
        {
            var currentLeague = await _context.SportLeagues.FindAsync(id);
            if (currentLeague == null)
            {
                return NotFound();
            }

            // Find the league positioned before this one
            var prevLeague = await _context.SportLeagues
                .Where(l => l.DisplayOrder < currentLeague.DisplayOrder)
                .OrderByDescending(l => l.DisplayOrder)
                .FirstOrDefaultAsync();

            // Swap display orders if we found a previous league
            if (prevLeague != null)
            {
                // Temporarily store current order
                var tempOrder = currentLeague.DisplayOrder;
                
                // Swap the orders
                currentLeague.DisplayOrder = prevLeague.DisplayOrder;
                prevLeague.DisplayOrder = tempOrder;
                
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: BetSettings/MoveDownLeague/5
        [HttpPost]
        [Route("MoveDownLeague/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDownLeague(int id)
        {
            var currentLeague = await _context.SportLeagues.FindAsync(id);
            if (currentLeague == null)
            {
                return NotFound();
            }

            // Find the league positioned after this one
            var nextLeague = await _context.SportLeagues
                .Where(l => l.DisplayOrder > currentLeague.DisplayOrder)
                .OrderBy(l => l.DisplayOrder)
                .FirstOrDefaultAsync();

            // Swap display orders if we found a next league
            if (nextLeague != null)
            {
                // Temporarily store current order
                var tempOrder = currentLeague.DisplayOrder;
                
                // Swap the orders
                currentLeague.DisplayOrder = nextLeague.DisplayOrder;
                nextLeague.DisplayOrder = tempOrder;
                
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        #endregion
        
        #region BetType Actions
        
        // GET: BetSettings/CreateBetType
        [HttpGet]
        [Route("CreateBetType")]
        public IActionResult CreateBetType()
        {
            return View();
        }        // POST: BetSettings/CreateBetType
        [HttpPost]
        [Route("CreateBetType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBetType([Bind("Id,BetType,DisplayName,Description,IsActive")] BetTypeConfiguration betTypeConfig)
        {
            if (ModelState.IsValid)
            {
                // Set default display order to end of list
                if (await _context.BetTypeConfigurations.AnyAsync())
                {
                    betTypeConfig.DisplayOrder = await _context.BetTypeConfigurations.MaxAsync(b => b.DisplayOrder) + 1;
                }
                else
                {
                    betTypeConfig.DisplayOrder = 1;
                }
                
                _context.Add(betTypeConfig);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(betTypeConfig);
        }
        
        // GET: BetSettings/EditBetType/5
        [HttpGet]
        [Route("EditBetType/{id}")]
        public async Task<IActionResult> EditBetType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var betType = await _context.BetTypeConfigurations.FindAsync(id);
            if (betType == null)
            {
                return NotFound();
            }
            return View(betType);
        }        // POST: BetSettings/EditBetType/5
        [HttpPost]
        [Route("EditBetType/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBetType(int id, [Bind("Id,BetType,DisplayName,Description,DisplayOrder,IsActive")] BetTypeConfiguration betTypeConfig)
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
                    if (!BetTypeExists(betTypeConfig.Id))
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
        
        // GET: BetSettings/DeleteBetType/5
        [HttpGet]
        [Route("DeleteBetType/{id}")]
        public async Task<IActionResult> DeleteBetType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var betType = await _context.BetTypeConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (betType == null)
            {
                return NotFound();
            }

            return View(betType);
        }
        
        // POST: BetSettings/DeleteBetType/5
        [HttpPost]
        [Route("DeleteBetType/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBetTypeConfirmed(int id)
        {
            var betType = await _context.BetTypeConfigurations.FindAsync(id);
            if (betType != null)
            {
                _context.BetTypeConfigurations.Remove(betType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // POST: BetSettings/MoveUpBetType/5
        [HttpPost]
        [Route("MoveUpBetType/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUpBetType(int id)
        {
            var currentType = await _context.BetTypeConfigurations.FindAsync(id);
            if (currentType == null)
            {
                return NotFound();
            }

            // Find the bet type positioned before this one
            var prevType = await _context.BetTypeConfigurations
                .Where(b => b.DisplayOrder < currentType.DisplayOrder)
                .OrderByDescending(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            // Swap display orders if we found a previous type
            if (prevType != null)
            {
                // Temporarily store current order
                var tempOrder = currentType.DisplayOrder;
                
                // Swap the orders
                currentType.DisplayOrder = prevType.DisplayOrder;
                prevType.DisplayOrder = tempOrder;
                
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // POST: BetSettings/MoveDownBetType/5
        [HttpPost]
        [Route("MoveDownBetType/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDownBetType(int id)
        {
            var currentType = await _context.BetTypeConfigurations.FindAsync(id);
            if (currentType == null)
            {
                return NotFound();
            }

            // Find the bet type positioned after this one
            var nextType = await _context.BetTypeConfigurations
                .Where(b => b.DisplayOrder > currentType.DisplayOrder)
                .OrderBy(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            // Swap display orders if we found a next type
            if (nextType != null)
            {
                // Temporarily store current order
                var tempOrder = currentType.DisplayOrder;
                
                // Swap the orders
                currentType.DisplayOrder = nextType.DisplayOrder;
                nextType.DisplayOrder = tempOrder;
                
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        #endregion
        
        private bool SportLeagueExists(int id)
        {
            return _context.SportLeagues.Any(e => e.Id == id);
        }
        
        private bool BetTypeExists(int id)
        {
            return _context.BetTypeConfigurations.Any(e => e.Id == id);
        }
    }
}
