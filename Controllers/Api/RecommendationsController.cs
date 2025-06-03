using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsBettingTracker.Models;
using SportsBettingTracker.Services;
using SportsBettingTracker.ViewModels;

namespace SportsBettingTracker.Controllers.Api
{    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly SportsBettingTracker.Services.BetRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecommendationsController(
            SportsBettingTracker.Services.BetRecommendationService recommendationService,
            UserManager<ApplicationUser> userManager)
        {
            _recommendationService = recommendationService;
            _userManager = userManager;
        }        /// <summary>
        /// Gets personalized bet recommendations based on betting history
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BetRecommendationViewModel>>> GetRecommendations([FromQuery] int limit = 5)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var serviceRecommendations = await _recommendationService.GetRecommendationsAsync(userId, limit);
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
            
            return Ok(viewModelRecommendations);
        }

        /// <summary>
        /// Calculates optimal bet size based on Kelly Criterion
        /// </summary>
        [HttpGet("kelly-calculator")]
        public ActionResult<KellyResult> CalculateKellyBet(
            [FromQuery] double bankroll, 
            [FromQuery] double winProbability, 
            [FromQuery] double odds)
        {
            if (bankroll <= 0 || winProbability <= 0 || winProbability >= 1 || odds <= 1)
            {
                return BadRequest("Invalid input parameters");
            }

            var kellyBet = _recommendationService.RecommendKellyBetSize(bankroll, winProbability, odds);
            
            return Ok(new KellyResult
            {
                RecommendedBetAmount = kellyBet,
                PercentOfBankroll = kellyBet / bankroll * 100,
                HalfKelly = kellyBet / 2,
                QuarterKelly = kellyBet / 4
            });
        }

        public class KellyResult
        {
            public double RecommendedBetAmount { get; set; }
            public double PercentOfBankroll { get; set; }
            public double HalfKelly { get; set; }
            public double QuarterKelly { get; set; }
        }
    }
}
