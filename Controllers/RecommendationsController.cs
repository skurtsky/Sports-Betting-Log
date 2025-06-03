using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsBettingTracker.Models;
using SportsBettingTracker.Services;
using SportsBettingTracker.ViewModels;

namespace SportsBettingTracker.Controllers
{    [Authorize]
    public class RecommendationsController : Controller
    {
        private readonly SportsBettingTracker.Services.BetRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecommendationsController(
            SportsBettingTracker.Services.BetRecommendationService recommendationService,
            UserManager<ApplicationUser> userManager)
        {
            _recommendationService = recommendationService;
            _userManager = userManager;
        }        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var serviceRecommendations = await _recommendationService.GetRecommendationsAsync(userId);
            
            // Map service model to view model
            var viewModelRecommendations = serviceRecommendations.Select(r => new BetRecommendationViewModel
            {
                Type = (RecommendationTypeViewModel)r.Type,
                SportLeagueId = r.SportLeagueId,
                SportLeagueName = r.SportLeagueName,
                BetTypeName = r.BetTypeName,
                TeamName = r.TeamName,
                Confidence = r.Confidence,
                Reasoning = r.Reasoning,
                SuggestedAction = r.SuggestedAction
            }).ToList();
            
            return View(viewModelRecommendations);
        }        [HttpGet]
        public IActionResult Calculator()
        {
            return View(new BetCalculatorViewModel());
        }

        [HttpPost]
        public IActionResult Calculator(BetCalculatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Convert win percentage to probability (0-1 scale)
            double winProbability = model.EstimatedWinPercentage / 100.0;
            
            // Calculate recommended bet size
            double recommendedSize = _recommendationService.RecommendKellyBetSize(
                model.CurrentBankroll,
                winProbability,
                model.DecimalOdds);

            model.RecommendedBetAmount = recommendedSize;
            model.CalculationComplete = true;

            return View(model);
        }
    }

    public class BetCalculatorModel
    {
        public double CurrentBankroll { get; set; }
        public double EstimatedWinPercentage { get; set; }
        public double DecimalOdds { get; set; }
        public double RecommendedBetAmount { get; set; }
        public bool CalculationComplete { get; set; } = false;
    }
}
