using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Extensions;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using Microsoft.VisualBasic.FileIO; // Add this for TextFieldParser
using System.IO; // For file handling

namespace SportsBettingTracker.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class BetsController : Controller
    {        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public BetsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }          // Helper method to save filter state to TempData
        private void SaveFilterState(string? sortOrder, string? searchString, int? pageNumber, 
            int? pageSize, string? dateFilter, string? sportFilter, string? betTypeFilter, string? resultFilter)
        {
            TempData["FilterSortOrder"] = sortOrder;
            TempData["FilterSearchString"] = searchString;
            TempData["FilterPageNumber"] = pageNumber;
            TempData["FilterPageSize"] = pageSize;
            TempData["FilterDateFilter"] = dateFilter;
            TempData["FilterSportFilter"] = sportFilter;
            TempData["FilterBetTypeFilter"] = betTypeFilter;
            TempData["FilterResultFilter"] = resultFilter;
            
            // Set a flag to indicate filters are applied
            TempData["FiltersApplied"] = true;
        }
        
        // Helper method to clear filter state from TempData
        private void ClearFilterState()
        {
            TempData.Remove("FilterSortOrder");
            TempData.Remove("FilterSearchString");
            TempData.Remove("FilterPageNumber");
            TempData.Remove("FilterPageSize");
            TempData.Remove("FilterDateFilter");
            TempData.Remove("FilterSportFilter");
            TempData.Remove("FilterBetTypeFilter");
            TempData.Remove("FilterResultFilter");
            TempData.Remove("FiltersApplied");
        }
          // Helper method to redirect to Index with preserved filters
        private IActionResult RedirectToIndexWithFilters()
        {
            if (TempData.ContainsKey("FiltersApplied"))
            {
                var isFiltersApplied = TempData.Peek("FiltersApplied") as bool?;
                if (isFiltersApplied.HasValue && isFiltersApplied.Value)
                {
                    // Keep TempData for the next request by using Peek instead of accessing directly
                    return RedirectToAction(nameof(Index));
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
          // GET: Bets
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber, int? pageSize, string? dateFilter, string? sportFilter, string? betTypeFilter, string? resultFilter, bool resetFilters = false)
        {
            // If reset is requested or if this is a direct navigation from the menu (no parameters and no saved filters), clear any saved filters
            if (resetFilters || (Request.Query.Count == 0 && !TempData.ContainsKey("FiltersApplied")))
            {
                ClearFilterState();
            }
            // If we have no explicit parameters but have saved filters, use those
            else if (Request.Query.Count == 0 && TempData.ContainsKey("FiltersApplied"))
            {
                sortOrder = TempData["FilterSortOrder"] as string;
                searchString = TempData["FilterSearchString"] as string;
                pageNumber = TempData["FilterPageNumber"] as int?;
                pageSize = TempData["FilterPageSize"] as int?;
                dateFilter = TempData["FilterDateFilter"] as string;
                sportFilter = TempData["FilterSportFilter"] as string;
                betTypeFilter = TempData["FilterBetTypeFilter"] as string;
                resultFilter = TempData["FilterResultFilter"] as string;
            }

            // Save the current filter state for the next request
            SaveFilterState(sortOrder, searchString, pageNumber, pageSize, dateFilter, sportFilter, betTypeFilter, resultFilter);            ViewData["CurrentSort"] = sortOrder;
            // Sort parameters
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["SportSortParm"] = sortOrder == "Sport" ? "sport_desc" : "Sport";
            ViewData["BetTypeSortParm"] = sortOrder == "BetType" ? "bettype_desc" : "BetType";
            ViewData["MatchSortParm"] = sortOrder == "Match" ? "match_desc" : "Match";
            ViewData["SelectionSortParm"] = sortOrder == "Selection" ? "selection_desc" : "Selection";
            ViewData["StakeSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Stake" : (sortOrder == "Stake" ? "stake_desc" : "Stake");
            ViewData["OddsSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Odds" : (sortOrder == "Odds" ? "odds_desc" : "Odds");
            ViewData["ResultSortParm"] = sortOrder == "Result" ? "result_desc" : "Result";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";

            // Filter values
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFilter"] = dateFilter;
            ViewData["SportFilter"] = sportFilter;
            ViewData["BetTypeFilter"] = betTypeFilter;
            ViewData["ResultFilter"] = resultFilter;

            // Page size options
            var pageSizeOptions = new[] { 10, 25, 50, 75, 100 };
            ViewData["PageSizeOptions"] = pageSizeOptions;
            ViewData["CurrentPageSize"] = pageSize ?? 10;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
              // Add sport leagues list for quick edit dropdowns
            ViewBag.SportLeagues = await _context.SportLeagues.OrderBy(s => s.Name).ToListAsync();            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;
            
            // Base query with the include
            IQueryable<Bet> betsQuery = _context.Bets.Include(b => b.SportLeague);
              // Filter bets by current user - always restrict to the current user's bets
            if (userId != null)
            {
                betsQuery = betsQuery.Where(b => b.UserId == userId);
            }            var bets = betsQuery;

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                bets = bets.Where(b => 
                    b.Match.Contains(searchString) ||
                    b.BetSelection.Contains(searchString) ||
                    (b.SportLeague != null && b.SportLeague.Name.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(dateFilter))
            {
                var today = DateTime.Today;
                bets = dateFilter switch
                {
                    "today" => bets.Where(b => b.BetDate.Date == today),
                    "yesterday" => bets.Where(b => b.BetDate.Date == today.AddDays(-1)),
                    "last7days" => bets.Where(b => b.BetDate.Date >= today.AddDays(-7)),
                    "last30days" => bets.Where(b => b.BetDate.Date >= today.AddDays(-30)),
                    "thismonth" => bets.Where(b => b.BetDate.Month == today.Month && b.BetDate.Year == today.Year),
                    "lastmonth" => bets.Where(b => b.BetDate.Month == today.AddMonths(-1).Month && b.BetDate.Year == today.AddMonths(-1).Year),
                    _ => bets
                };
            }

            if (!string.IsNullOrEmpty(sportFilter))
            {
                bets = bets.Where(b => b.SportLeague != null && b.SportLeague.Id.ToString() == sportFilter);
            }

            if (!string.IsNullOrEmpty(betTypeFilter))
            {
                if (Enum.TryParse<Models.BetType>(betTypeFilter, out var betType))
                {
                    bets = bets.Where(b => b.BetType == betType);
                }
            }

            if (!string.IsNullOrEmpty(resultFilter))
            {
                if (Enum.TryParse<Models.BetResult>(resultFilter, out var result))
                {
                    bets = bets.Where(b => b.Result == result);
                }
            }            // Apply sorting
            bets = (sortOrder ?? string.Empty) switch
            {
                "date_desc" => bets.OrderByDescending(b => b.BetDate),
                "Sport" => bets.OrderBy(b => b.SportLeague != null ? b.SportLeague.Name : ""),
                "sport_desc" => bets.OrderByDescending(b => b.SportLeague != null ? b.SportLeague.Name : ""),
                "BetType" => bets.OrderBy(b => b.BetType),
                "bettype_desc" => bets.OrderByDescending(b => b.BetType),
                "Match" => bets.OrderBy(b => b.Match),
                "match_desc" => bets.OrderByDescending(b => b.Match),
                "Selection" => bets.OrderBy(b => b.BetSelection),
                "selection_desc" => bets.OrderByDescending(b => b.BetSelection),
                "Stake" => bets.OrderBy(b => b.Stake),
                "stake_desc" => bets.OrderByDescending(b => b.Stake),
                "Odds" => bets.OrderBy(b => b.Odds),
                "odds_desc" => bets.OrderByDescending(b => b.Odds),
                "Result" => bets.OrderBy(b => b.Result),
                "result_desc" => bets.OrderByDescending(b => b.Result),
                "Amount" => bets.OrderBy(b => b.AmountWonLost),
                "amount_desc" => bets.OrderByDescending(b => b.AmountWonLost),
                _ => bets.OrderBy(b => b.BetDate),
            };var selectedPageSize = pageSize ?? 10;
            return View(await PaginatedList<Bet>.CreateAsync(bets.AsNoTracking(), pageNumber ?? 1, selectedPageSize));
        }        // GET: Bets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;

            var bet = await _context.Bets
                .Include(b => b.SportLeague)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (bet == null)
            {
                return NotFound();
            }
            
            // Check if user owns the bet - strict ownership check
            bool canAccess = bet.UserId == userId;
            if (!canAccess)
            {
                return Forbid();
            }

            return View(bet);
        }

        // GET: Bets/Create
        public IActionResult Create()
        {
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name");
            return View();
        }        // POST: Bets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BetDate,SportLeagueId,BetType,Match,BetSelection,Stake,Odds,Result")] Bet bet)
        {            if (ModelState.IsValid)
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(User);
                
                // Always associate bet with the current user, including demo users
                if (currentUser != null)
                {
                    bet.UserId = currentUser.Id;
                }
                  bet.CalculateWinLoss();
                _context.Add(bet);
                await _context.SaveChangesAsync();
                return RedirectToIndexWithFilters();
            }
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }        // GET: Bets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;

            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }
              // Check if user owns the bet - strict ownership check
            bool canAccess = bet.UserId == userId;
            if (!canAccess)
            {
                return Forbid();
            }
            
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }        // POST: Bets/Edit/5
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("Id,BetDate,SportLeagueId,BetType,Match,BetSelection,Stake,Odds,Result")] Bet bet)
        {
            if (id != bet.Id)
            {
                return NotFound();
            }
            
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;
            
            // Get original bet to check ownership
            var originalBet = await _context.Bets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (originalBet == null)
            {
                return NotFound();
            }
              // Check if user owns the bet - strict ownership check
            bool canAccess = originalBet.UserId == userId;
            if (!canAccess)
            {
                return Forbid();
            }
            
            // Preserve the UserId from the original bet
            bet.UserId = originalBet.UserId;

            if (ModelState.IsValid)
            {
                try
                {                    bet.CalculateWinLoss();
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
                return RedirectToIndexWithFilters();
            }
            ViewData["SportLeagueId"] = new SelectList(_context.SportLeagues, "Id", "Name", bet.SportLeagueId);
            return View(bet);
        }        // GET: Bets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;

            var bet = await _context.Bets
                .Include(b => b.SportLeague)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bet == null)
            {
                return NotFound();
            }
            
            // Check if user owns the bet - strict ownership check
            bool canAccess = bet.UserId == userId;
            if (!canAccess)
            {
                return Forbid();
            }

            return View(bet);
        }        // POST: Bets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            bool isDemoUser = currentUser?.IsDemoUser ?? false;
            
            var bet = await _context.Bets.FindAsync(id);
            if (bet != null)
            {                // Check if user owns the bet - strict ownership check
                bool canAccess = bet.UserId == userId;
                if (!canAccess)
                {
                    return Forbid();
                }                _context.Bets.Remove(bet);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToIndexWithFilters();
        }        // GET: Bets/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;
            
            // Only export bets that belong to the current user
            var bets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == userId)
                .ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Id,BetDate,SportLeague,BetType,Match,BetSelection,Stake,Odds,Result,AmountWonLost");
            foreach (var b in bets)
            {
                csv.AppendLine($"{b.Id}," +
                    $"{b.BetDate:yyyy-MM-dd}," +
                    $"{b.SportLeague?.Name}," +
                    $"{b.BetType}," +
                    $"{b.Match.Replace(",", " ")}," +
                    $"{b.BetSelection.Replace(",", " ")}," +
                    $"{b.Stake}," +
                    $"{b.Odds}," +
                    $"{b.Result}," +
                    $"{b.AmountWonLost}");
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"bets_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // GET: Bets/ImportCsv
        public IActionResult ImportCsv()
        {
            return View();
        }

        // POST: Bets/ImportCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(Microsoft.AspNetCore.Http.IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("csvFile", "Please select a CSV file.");
                return View();
            }

            // Create a unique temp file path for the uploaded CSV
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"sbt_import_{Guid.NewGuid()}.csv");
            
            try
            {
                // Save the CSV file to disk
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await csvFile.CopyToAsync(fileStream);
                }
                
                // Read just the headers and first 10 rows for preview
                using var reader = new StreamReader(tempFilePath);
                using var csvParser = new TextFieldParser(reader);
                csvParser.TextFieldType = FieldType.Delimited;
                csvParser.SetDelimiters(",");
                csvParser.HasFieldsEnclosedInQuotes = true;
                
                // Read the headers
                var headers = csvParser.ReadFields();
                if (headers == null || headers.Length == 0)
                {
                    ModelState.AddModelError("csvFile", "CSV file has no headers.");
                    System.IO.File.Delete(tempFilePath);
                    return View();
                }
                
                // Read up to 10 lines for preview
                var previewRows = new List<string[]>();
                int rowCount = 0;
                while (!csvParser.EndOfData && rowCount < 10)
                {
                    var fields = csvParser.ReadFields();
                    if (fields != null && fields.Any(f => !string.IsNullOrWhiteSpace(f)))
                    {
                        previewRows.Add(fields);
                        rowCount++;
                    }
                }
                
                // Check if we have at least one data row
                if (previewRows.Count < 1)
                {
                    ModelState.AddModelError("csvFile", "CSV file must have at least a header and one data row.");
                    System.IO.File.Delete(tempFilePath);
                    return View();
                }
                
                // Store the file path in TempData for the mapping step
                TempData["CsvFilePath"] = tempFilePath;
                
                // Store the headers and preview rows for mapping UI
                TempData["CsvHeaders"] = System.Text.Json.JsonSerializer.Serialize(headers);
                
                // Convert the preview rows to CSV strings for display
                var previewRowsFormatted = previewRows
                    .Select(row => string.Join(",", row.Select(field => field.Contains(",") ? $"\"{field}\"" : field)))
                    .ToList();
                TempData["CsvRows"] = System.Text.Json.JsonSerializer.Serialize(previewRowsFormatted);
                
                return RedirectToAction("MapCsvColumns");
            }
            catch (Exception ex)
            {
                // Clean up the temp file if an error occurred
                if (System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); } catch { /* Ignore deletion errors */ }
                }
                
                ModelState.AddModelError("", $"Error processing CSV file: {ex.Message}");
                return View();
            }
        }

        // GET: Bets/MapCsvColumns
        public IActionResult MapCsvColumns()
        {
            // Make sure we have the CSV file path and headers
            if (TempData["CsvFilePath"] == null || TempData["CsvHeaders"] == null || TempData["CsvRows"] == null)
                return RedirectToAction("ImportCsv");
            
            var csvFilePath = TempData["CsvFilePath"] as string;
            var headersJson = TempData["CsvHeaders"] as string;
            var rowsJson = TempData["CsvRows"] as string;
            
            if (csvFilePath == null || headersJson == null || rowsJson == null)
                return RedirectToAction("ImportCsv");
                
            // Verify the temp file still exists
            if (!System.IO.File.Exists(csvFilePath))
            {
                TempData["Error"] = "The uploaded CSV file is no longer available. Please upload again.";
                return RedirectToAction("ImportCsv");
            }
            
            // Keep the file path in TempData for the next request
            TempData.Keep("CsvFilePath");
            
            var headers = System.Text.Json.JsonSerializer.Deserialize<string[]>(headersJson) ?? Array.Empty<string>();
            var rows = System.Text.Json.JsonSerializer.Deserialize<List<string>>(rowsJson) ?? new List<string>();
            
            // Create auto-mapping dictionary with partial matches
            var autoMappings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "BetDate", new List<string> { "date", "betdate", "bet date", "game date", "gamedate", "date placed", "placed" } },
                { "SportLeague", new List<string> { "league", "sport", "sportleague", "sport/league", "sport / league", "category", "competition" } },
                { "Match", new List<string> { "match", "game", "event", "fixture", "matchup", "teams", "description", "match details" } },
                { "BetSelection", new List<string> { "selection", "pick", "bet", "betselection", "bet selection", "wager type", "bet details", "pick details" } },
                { "Stake", new List<string> { "stake", "wager", "amount", "bet amount", "risk", "risked", "amount staked", "wagered" } },
                { "Odds", new List<string> { "odds", "line", "price", "american odds", "odds (american)", "money line", "moneyline" } },
                { "Result", new List<string> { "result", "outcome", "won/lost", "win/loss", "status", "bet result", "w/l", "win/loss/push", "w/l/p" } },
                { "AmountWonLost", new List<string> { "amount won", "amount won/lost", "won", "profit", "payout", "winnings", "profit/loss", "net result", "win/loss amount" } },
                { "BetType", new List<string> { "type", "bet type", "wager type", "market" } }
            };
            
            // Generate default mappings based on header names
            var suggestedMappings = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i].Trim();
                // First try exact match with field name
                if (autoMappings.ContainsKey(header))
                {
                    suggestedMappings[i] = header;
                    continue;
                }
                // Then try matching against all possible values
                foreach (var mapping in autoMappings)
                {
                    if (mapping.Value.Any(v => v.Equals(header, StringComparison.OrdinalIgnoreCase)))
                    {
                        suggestedMappings[i] = mapping.Key;
                        break;
                    }
                }
            }
              ViewBag.Headers = headers;
            ViewBag.Rows = rows;
            ViewBag.SuggestedMappings = suggestedMappings;
            ViewBag.BetFields = new[] { "BetDate", "SportLeague", "BetType", "Match", "BetSelection", "Stake", "Odds", "Result", "AmountWonLost" };
            ViewBag.RequiredFields = new[] { "BetDate", "Match", "BetSelection", "Stake", "Odds" };
            ViewBag.CsvFilePath = csvFilePath;
            ViewBag.MissingFields = TempData["MissingFields"];
            ViewBag.DateFormats = new[] { 
                "MM/dd/yyyy", 
                "dd/MM/yyyy",
                "yyyy-MM-dd",
                "MM-dd-yyyy",
                "dd-MM-yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };
            return View();
        }        // POST: Bets/ImportCsvMapped
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsvMapped(string csvFilePath, string[] columnMap, string dateFormat)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath) || !System.IO.File.Exists(csvFilePath) || columnMap == null || columnMap.Length == 0)
            {
                TempData["Error"] = "Invalid mapping or CSV file not found.";
                return RedirectToAction("ImportCsv");
            }

            try
            {
                // Debug information
                var mappingInfo = new List<string>();
                for (int i = 0; i < columnMap.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(columnMap[i]))
                        mappingInfo.Add($"Column {i}: {columnMap[i]}");
                }
                TempData["Debug"] = $"Column mappings: {string.Join(", ", mappingInfo)}";
                
                // Read just the header row to validate the mapping
                string[] csvHeaders;
                using (var reader = new StreamReader(csvFilePath))
                using (var parser = new TextFieldParser(reader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    
                    csvHeaders = parser.ReadFields() ?? Array.Empty<string>();
                    
                    if (csvHeaders.Length == 0)
                    {
                        TempData["Error"] = "CSV file has no headers.";
                        return RedirectToAction("ImportCsv");
                    }
                }
                
                // Check for required fields
                var requiredFields = new[] { "BetDate", "Match", "BetSelection", "Stake", "Odds" };
                var missingFields = requiredFields.Where(rf => !columnMap.Contains(rf)).ToList();
                if (missingFields.Any())
                {
                    // Re-read the headers and preview rows for re-display
                    var previewRows = new List<string>();
                    using (var reader = new StreamReader(csvFilePath))
                    {
                        // Skip header row
                        var headerLine = reader.ReadLine();
                        
                        // Get up to 10 preview rows
                        for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            previewRows.Add(line);
                        }
                    }
                    
                    TempData["MissingFields"] = string.Join(",", missingFields);
                    TempData["CsvHeaders"] = System.Text.Json.JsonSerializer.Serialize(csvHeaders);
                    TempData["CsvRows"] = System.Text.Json.JsonSerializer.Serialize(previewRows);
                    TempData["CsvFilePath"] = csvFilePath;
                    return RedirectToAction("MapCsvColumns");
                }
                
                // Process the import
                var imported = 0;
                var skipped = 0;
                var errors = new List<string>();

                // Process the CSV file row by row to avoid loading it all into memory
                using (var reader = new StreamReader(csvFilePath))
                using (var parser = new TextFieldParser(reader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    
                    // Skip the header row
                    parser.ReadFields();
                    
                    int rowIndex = 1; // Start at line 1 (after header)
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            var cells = parser.ReadFields();
                            if (cells == null || cells.Length != csvHeaders.Length) 
                            {
                                skipped++;
                                errors.Add($"Row {rowIndex}: Column count mismatch or empty row");
                                rowIndex++;
                                continue;
                            }

                            var bet = new Models.Bet();
                            string? leagueName = null;
                            string? betTypeStr = null;
                            bool stakeSet = false, oddsSet = false;
                            bool hasRequiredFields = true;
                            var missingFieldsList = new List<string>();

                            for (int c = 0; c < Math.Min(columnMap.Length, cells.Length); c++)
                            {
                                var field = columnMap[c];
                                if (string.IsNullOrWhiteSpace(field)) continue;
                                
                                var value = cells[c]?.Trim() ?? string.Empty;
                                  switch (field)
                                {
                                    case "BetDate":
                                        if (DateTime.TryParseExact(value, dateFormat, System.Globalization.CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None, out var dt)) 
                                            bet.BetDate = dt;
                                        else
                                            missingFieldsList.Add($"BetDate (format: {dateFormat})");
                                        break;
                                    case "SportLeague":
                                        leagueName = value;
                                        break;
                                    case "BetType":
                                        betTypeStr = value;
                                        break;
                                    case "Match":
                                        if (!string.IsNullOrWhiteSpace(value))
                                            bet.Match = value;
                                        else
                                            missingFieldsList.Add("Match");
                                        break;
                                    case "BetSelection":
                                        if (!string.IsNullOrWhiteSpace(value))
                                            bet.BetSelection = value;
                                        else
                                            missingFieldsList.Add("BetSelection");
                                        break;
                                    case "Stake":
                                        // Remove currency symbol, commas, and whitespace
                                        var stakeStr = value.Replace("$", "").Replace(",", "").Trim();
                                        if (decimal.TryParse(stakeStr, out var stake)) 
                                        { 
                                            bet.Stake = stake; 
                                            stakeSet = true; 
                                        }
                                        else
                                            missingFieldsList.Add("Stake");
                                        break;
                                    case "Odds":
                                        var oddsStr = value.Replace("+", "").Trim();
                                        if (int.TryParse(oddsStr, out var odds)) 
                                        { 
                                            bet.Odds = odds; 
                                            oddsSet = true; 
                                        }
                                        else
                                            missingFieldsList.Add("Odds");
                                        break;
                                    case "Result":
                                        // Clean up the value and convert to uppercase for consistent handling
                                        value = value.Trim().ToUpper();
                                        
                                        // Handle special cases first
                                        if (value == "P" || value == "PUSH" || value == "PUSHED" || value == "T" || value == "TIE")
                                        {
                                            bet.Result = Models.BetResult.PUSH;
                                        }
                                        else if (value == "W" || value == "WIN" || value == "WON" || value == "1")
                                        {
                                            bet.Result = Models.BetResult.WIN;
                                        }
                                        else if (value == "L" || value == "LOSS" || value == "LOST" || value == "0")
                                        {
                                            bet.Result = Models.BetResult.LOSS;
                                        }
                                        // If none of the special cases match, try standard enum parsing
                                        else if (!Enum.TryParse<Models.BetResult>(value, out var res))
                                        {
                                            bet.Result = Models.BetResult.PENDING;
                                        }
                                        else
                                        {
                                            bet.Result = res;
                                        }
                                        break;
                                    case "AmountWonLost":
                                        var amountStr = value.Replace("$", "").Replace(",", "").Replace("+", "").Trim();
                                        if (string.IsNullOrWhiteSpace(amountStr)) break;
                                        if (decimal.TryParse(amountStr, out var awl))
                                        {
                                            // If amount starts with "-", it's already negative
                                            bet.AmountWonLost = value.StartsWith("-") ? awl : Math.Abs(awl);
                                        }
                                        break;
                                }
                            }
                            
                            // Handle SportLeague
                            if (string.IsNullOrWhiteSpace(leagueName)) leagueName = "Other";
                            var league = await _context.SportLeagues.FirstOrDefaultAsync(l => l.Name == leagueName);
                            if (league == null)
                            {
                                league = new Models.SportLeague { 
                                    Name = leagueName, 
                                    IsActive = true, 
                                    DisplayOrder = _context.SportLeagues.Any() ? _context.SportLeagues.Max(s => s.DisplayOrder) + 1 : 1 
                                };
                                _context.SportLeagues.Add(league);
                                await _context.SaveChangesAsync();
                            }
                            bet.SportLeagueId = league.Id;
                            
                            // Handle BetType
                            Models.BetType betTypeEnum;
                            if (string.IsNullOrWhiteSpace(betTypeStr) || !Enum.TryParse<Models.BetType>(betTypeStr, out betTypeEnum))
                                betTypeEnum = Models.BetType.Other;
                            bet.BetType = betTypeEnum;
                            
                            // Calculate AmountWonLost if not provided
                            if (!bet.AmountWonLost.HasValue && stakeSet && oddsSet)
                            {
                                bet.CalculateWinLoss();
                            }

                            // Check required fields
                            hasRequiredFields = bet.BetDate != default && 
                                               !string.IsNullOrWhiteSpace(bet.Match) && 
                                               !string.IsNullOrWhiteSpace(bet.BetSelection) && 
                                               stakeSet && oddsSet;                            if (hasRequiredFields)
                            {
                                // Add the current user ID to the bet
                                var currentUser = await _userManager.GetUserAsync(User);
                                
                                // Always associate with current user - this is critical for data isolation
                                if (currentUser != null)
                                {
                                    bet.UserId = currentUser.Id;
                                }
                                
                                _context.Bets.Add(bet);
                                imported++;
                                
                                // Save in batches of 50 to avoid memory issues with very large files
                                if (imported % 50 == 0)
                                {
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                skipped++;
                                if (missingFieldsList.Any())
                                {
                                    errors.Add($"Row {rowIndex}: Missing or invalid fields: {string.Join(", ", missingFieldsList)}");
                                }
                                else
                                {
                                    errors.Add($"Row {rowIndex}: Missing required fields");
                                }
                            }
                            
                            rowIndex++;
                        }
                        catch (Exception ex)
                        {
                            skipped++;
                            errors.Add($"Row {rowIndex}: {ex.Message}");
                            rowIndex++;
                        }
                    }
                }
                
                // Save any remaining changes
                if (imported > 0 && imported % 50 != 0)
                {
                    await _context.SaveChangesAsync();
                }
                
                if (imported > 0)
                    TempData["Success"] = $"Successfully imported {imported} bets.";
                else
                    TempData["Warning"] = "No bets were imported. Please check your data.";
                
                if (skipped > 0)
                    TempData["Info"] = $"Skipped {skipped} rows due to missing or invalid data.";
                
                if (errors.Count > 0)
                    TempData["ImportErrors"] = string.Join("<br>", errors.Take(5));
            }
            finally
            {
                // Clean up the temporary file
                try
                {
                    if (System.IO.File.Exists(csvFilePath))
                    {
                        System.IO.File.Delete(csvFilePath);
                    }
                }
                catch
                {
                    // Ignore errors during cleanup
                }            }
            
            return RedirectToIndexWithFilters();
        }

        // POST: Bets/QuickEdit - API endpoint for in-line editing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickEdit(int id, string field, string value)
        {
            var bet = await _context.Bets.Include(b => b.SportLeague).FirstOrDefaultAsync(b => b.Id == id);
            if (bet == null)
            {
                return Json(new { success = false, message = "Bet not found" });
            }

            try
            {
                switch (field)
                {
                    case "SportLeagueId":
                        if (int.TryParse(value, out int sportLeagueId))
                        {
                            var league = await _context.SportLeagues.FindAsync(sportLeagueId);
                            if (league == null)
                                return Json(new { success = false, message = "Sport/League not found" });
                            bet.SportLeagueId = sportLeagueId;
                            bet.SportLeague = league; // Update the navigation property for the response
                        }
                        else
                            return Json(new { success = false, message = "Invalid Sport/League selection" });
                        break;
                    case "BetDate":
                        if (DateTime.TryParse(value, out DateTime date))
                            bet.BetDate = date;
                        else
                            return Json(new { success = false, message = "Invalid date format" });
                        break;
                    case "Match":
                        if (!string.IsNullOrWhiteSpace(value))
                            bet.Match = value;
                        else
                            return Json(new { success = false, message = "Match cannot be empty" });
                        break;
                    case "BetSelection":
                        if (!string.IsNullOrWhiteSpace(value))
                            bet.BetSelection = value;
                        else
                            return Json(new { success = false, message = "Selection cannot be empty" });
                        break;
                    case "Stake":
                        if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out decimal stake))
                            bet.Stake = stake;
                        else
                            return Json(new { success = false, message = "Invalid stake format" });
                        break;
                    case "Odds":
                        var oddsStr = value.Replace("+", "");
                        if (int.TryParse(oddsStr, out int odds))
                            bet.Odds = odds;
                        else
                            return Json(new { success = false, message = "Invalid odds format" });
                        break;
                    case "Result":
                        if (Enum.TryParse<BetResult>(value, out BetResult result))
                            bet.Result = result;
                        else
                            return Json(new { success = false, message = "Invalid result value" });
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid field" });
                }

                // Recalculate amount won/lost
                bet.CalculateWinLoss();
                
                await _context.SaveChangesAsync();
                
                // Return formatted values for display
                var formattedValue = field switch
                {
                    "BetDate" => bet.BetDate.ToShortDateString(),
                    "Odds" => bet.FormattedOdds,
                    "Stake" => $"${bet.Stake:F2}",
                    "Result" => bet.Result.ToString(),
                    "SportLeagueId" => bet.SportLeague?.Name ?? "Unknown",
                    _ => value
                };
                
                return Json(new { 
                    success = true, 
                    formattedValue = formattedValue, 
                    amountWonLost = bet.FormattedAmountWonLost,
                    resultClass = bet.Result == BetResult.WIN ? "bg-success" : 
                                  bet.Result == BetResult.LOSS ? "bg-danger" : 
                                  bet.Result == BetResult.PUSH ? "bg-secondary" : "bg-warning"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Bets/BulkEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkEdit([FromBody] BulkEditModel model)
        {
            if (model == null || model.Ids == null || !model.Ids.Any())
            {
                return Json(new { success = false, message = "No bets selected" });
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;

            try
            {
                var bets = await _context.Bets
                    .Where(b => model.Ids.Contains(b.Id) && b.UserId == userId)
                    .ToListAsync();

                foreach (var bet in bets)
                {                    if (!string.IsNullOrEmpty(model.Result))
                    {
                        if (Enum.TryParse<BetResult>(model.Result, out var result))
                        {
                            bet.Result = result;
                            bet.CalculateWinLoss();
                        }
                    }

                    if (!string.IsNullOrEmpty(model.SportLeagueId) && int.TryParse(model.SportLeagueId, out var sportLeagueId))
                    {
                        var league = await _context.SportLeagues.FindAsync(sportLeagueId);
                        if (league != null)
                        {
                            bet.SportLeagueId = sportLeagueId;
                        }
                    }

                    if (!string.IsNullOrEmpty(model.BetType))
                    {
                        if (Enum.TryParse<BetType>(model.BetType, out var betType))
                        {
                            bet.BetType = betType;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Bets/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No bets selected" });
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            string? userId = currentUser?.Id;

            try
            {
                var bets = await _context.Bets
                    .Where(b => ids.Contains(b.Id) && b.UserId == userId)
                    .ToListAsync();

                _context.Bets.RemoveRange(bets);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool BetExists(int id)
        {
            return _context.Bets.Any(e => e.Id == id);
        }
    }
}
