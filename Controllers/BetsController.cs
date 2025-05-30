using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;
using Microsoft.VisualBasic.FileIO; // Add this for TextFieldParser

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
            {                bets = bets.Where(b => 
                    b.Match.Contains(searchString) ||
                    b.BetSelection.Contains(searchString) ||
                    (b.SportLeague != null && b.SportLeague.Name.Contains(searchString)));
            }            bets = sortOrder switch
            {
                "date_desc" => bets.OrderByDescending(b => b.BetDate),
                "Sport" => bets.OrderBy(b => b.SportLeague != null ? b.SportLeague.Name : ""),
                "sport_desc" => bets.OrderByDescending(b => b.SportLeague != null ? b.SportLeague.Name : ""),
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
        }        // POST: Bets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BetDate,SportLeagueId,BetType,Match,BetSelection,Stake,Odds,Result")] Bet bet)
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
        }        // POST: Bets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BetDate,SportLeagueId,BetType,Match,BetSelection,Stake,Odds,Result")] Bet bet)
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

        // GET: Bets/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var bets = await _context.Bets.Include(b => b.SportLeague).ToListAsync();
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
            }            // Read the CSV file using TextFieldParser to handle quoted fields
            using var stream = new System.IO.MemoryStream();
            await csvFile.CopyToAsync(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            using var csvParser = new TextFieldParser(reader);
            csvParser.TextFieldType = FieldType.Delimited;
            csvParser.SetDelimiters(",");
            csvParser.HasFieldsEnclosedInQuotes = true;

            var lines = new List<string[]>();
            while (!csvParser.EndOfData)
            {
                var fields = csvParser.ReadFields();
                if (fields != null && fields.Any(f => !string.IsNullOrWhiteSpace(f)))
                {
                    lines.Add(fields);
                }
            }

            if (lines.Count < 2)
            {
                ModelState.AddModelError("csvFile", "CSV file must have at least a header and one data row.");
                return View();
            }

            // Get headers from first row
            var headers = lines[0];

            // Store the data in TempData for mapping step
            TempData["CsvHeaders"] = System.Text.Json.JsonSerializer.Serialize(headers);
            // Convert the arrays to CSV strings for preview
            var previewRows = lines.Skip(1).Take(10)
                .Select(row => string.Join(",", row.Select(field => field.Contains(",") ? $"\"{field}\"" : field)))
                .ToList();
            TempData["CsvRows"] = System.Text.Json.JsonSerializer.Serialize(previewRows);
            
            // Store the raw CSV with proper quoting
            var csvContent = new System.Text.StringBuilder();
            foreach (var row in lines)
            {
                csvContent.AppendLine(string.Join(",", row.Select(field => field.Contains(",") ? $"\"{field}\"" : field)));
            }
            TempData["CsvRaw"] = csvContent.ToString();
            return RedirectToAction("MapCsvColumns");
        }

        // GET: Bets/MapCsvColumns
        public IActionResult MapCsvColumns()
        {            if (TempData["CsvHeaders"] == null || TempData["CsvRows"] == null)
                return RedirectToAction("ImportCsv");
            
            var headersJson = TempData["CsvHeaders"] as string;
            var rowsJson = TempData["CsvRows"] as string;
            if (headersJson == null || rowsJson == null)
                return RedirectToAction("ImportCsv");

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
            ViewBag.CsvRaw = TempData["CsvRaw"];
            ViewBag.MissingFields = TempData["MissingFields"];
            return View();
        }

        // POST: Bets/ImportCsvMapped
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsvMapped(string csvRaw, string[] columnMap)
        {
            if (string.IsNullOrWhiteSpace(csvRaw) || columnMap == null || columnMap.Length == 0)
            {
                TempData["Error"] = "Invalid mapping or CSV data.";
                return RedirectToAction("ImportCsv");
            }

            // Debug information
            var mappingInfo = new List<string>();
            for (int i = 0; i < columnMap.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(columnMap[i]))
                    mappingInfo.Add($"Column {i}: {columnMap[i]}");
            }
            TempData["Debug"] = $"Column mappings: {string.Join(", ", mappingInfo)}";
            
            var csvLines = csvRaw.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            if (csvLines.Count < 2)
            {
                TempData["Error"] = "CSV file must have at least a header and one data row.";
                return RedirectToAction("ImportCsv");
            }
            
            var csvHeaders = csvLines[0].Trim().Split(',');
            
            // Check for required fields
            var requiredFields = new[] { "BetDate", "Match", "BetSelection", "Stake", "Odds" };
            var missingFields = requiredFields.Where(rf => !columnMap.Contains(rf)).ToList();
            if (missingFields.Any())
            {
                var previewRows = csvLines.Skip(1).Take(10).ToList();
                TempData["MissingFields"] = string.Join(",", missingFields);
                TempData["CsvHeaders"] = System.Text.Json.JsonSerializer.Serialize(csvHeaders);
                TempData["CsvRows"] = System.Text.Json.JsonSerializer.Serialize(previewRows);
                TempData["CsvRaw"] = csvRaw;
                return RedirectToAction("MapCsvColumns");
            }
            
            // Process the import
            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();

            for (int i = 1; i < csvLines.Count; i++)
            {
                try
                {                    // Use TextFieldParser to properly handle quoted fields
                    using var reader = new StringReader(csvLines[i]);
                    using var parser = new TextFieldParser(reader);
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    var cells = parser.ReadFields();
                    if (cells == null || cells.Length != csvHeaders.Length) 
                    {
                        skipped++;
                        errors.Add($"Row {i}: Column count mismatch or empty row");
                        continue;
                    }

                    var bet = new Models.Bet();
                    string? leagueName = null;
                    string? betTypeStr = null;
                    bool stakeSet = false, oddsSet = false;
                    bool hasRequiredFields = true;
                    var missingFieldsList = new List<string>();

                    for (int c = 0; c < columnMap.Length; c++)
                    {
                        if (c >= cells.Length) continue;
                        
                        var field = columnMap[c];
                        if (string.IsNullOrWhiteSpace(field)) continue;
                        
                        var value = cells[c]?.Trim() ?? string.Empty;
                        
                        switch (field)
                        {
                            case "BetDate":
                                if (DateTime.TryParse(value, out var dt)) 
                                    bet.BetDate = dt;
                                else
                                    missingFieldsList.Add("BetDate");
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
                                {
                                    missingFieldsList.Add("Stake");
                                }
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
                    var league = _context.SportLeagues.FirstOrDefault(l => l.Name == leagueName);
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
                                       stakeSet && oddsSet;

                    if (hasRequiredFields)
                    {
                        _context.Bets.Add(bet);
                        imported++;
                    }
                    else
                    {
                        skipped++;
                        if (missingFieldsList.Any())
                        {
                            errors.Add($"Row {i}: Missing or invalid fields: {string.Join(", ", missingFieldsList)}");
                        }
                        else
                        {
                            errors.Add($"Row {i}: Missing required fields");
                        }
                    }
                }
                catch (Exception ex)
                {
                    skipped++;
                    errors.Add($"Row {i}: {ex.Message}");
                }
            }
            
            try
            {
                await _context.SaveChangesAsync();
                
                if (imported > 0)
                    TempData["Success"] = $"Successfully imported {imported} bets.";
                else
                    TempData["Warning"] = "No bets were imported. Please check your data.";
                
                if (skipped > 0)
                    TempData["Info"] = $"Skipped {skipped} rows due to missing or invalid data.";
                
                if (errors.Count > 0)
                    TempData["ImportErrors"] = string.Join("<br>", errors.Take(5));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving to database: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        private bool BetExists(int id)
        {
            return _context.Bets.Any(e => e.Id == id);
        }
    }
}
