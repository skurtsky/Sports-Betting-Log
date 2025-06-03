using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportsBettingTracker.Data;
using SportsBettingTracker.Models;
using SportsBettingTracker.ViewModels;

namespace SportsBettingTracker.Services
{
    public class BetRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public BetRecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets recommendations based on user's historical betting patterns
        /// </summary>
        public async Task<List<BetRecommendationViewModel>> GetRecommendationsAsync(string userId, int limit = 5)
        {
            // Get user's completed bets
            var userBets = await _context.Bets
                .Include(b => b.SportLeague)
                .Where(b => b.UserId == userId && b.Result != BetResult.PENDING)
                .ToListAsync();

            var recommendations = new List<BetRecommendationViewModel>();

            // If user doesn't have enough historical data, return empty list
            if (userBets.Count < 10)
            {
                return recommendations;
            }

            // Analyze winning bets by sport league
            var sportLeagueAnalysis = userBets
                .GroupBy(b => b.SportLeagueId)
                .Select(g => new
                {
                    SportLeagueId = g.Key,
                    SportLeagueName = g.First().SportLeague?.Name ?? "Unknown",
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    TotalProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    AverageOdds = g.Average(b => b.Odds)
                })
                .Where(g => g.TotalBets >= 3) // Only consider leagues with at least 3 bets
                .OrderByDescending(g => (double)g.WinningBets / g.TotalBets)
                .ThenByDescending(g => g.TotalProfit)
                .ToList();

            // Analyze winning bets by bet type
            var betTypeAnalysis = userBets
                .GroupBy(b => b.BetType)
                .Select(g => new
                {
                    BetType = g.Key,
                    BetTypeName = g.Key.ToString(),
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    TotalProfit = g.Sum(b => b.AmountWonLost ?? 0),
                    AverageOdds = g.Average(b => b.Odds)
                })
                .Where(g => g.TotalBets >= 3) // Only consider bet types with at least 3 bets
                .OrderByDescending(g => (double)g.WinningBets / g.TotalBets)
                .ThenByDescending(g => g.TotalProfit)
                .ToList();

            // Analyze team performance
            var teamPerformance = userBets
                .GroupBy(b => b.BetSelection)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new
                {
                    Team = g.Key,
                    TotalBets = g.Count(),
                    WinningBets = g.Count(b => b.Result == BetResult.WIN),
                    TotalProfit = g.Sum(b => b.AmountWonLost ?? 0)
                })
                .Where(g => g.TotalBets >= 2) // Only consider teams with at least 2 bets
                .OrderByDescending(g => (double)g.WinningBets / g.TotalBets)
                .ThenByDescending(g => g.TotalProfit)
                .ToList();

            // Generate recommendations based on best performing sport leagues
            foreach (var league in sportLeagueAnalysis.Take(3))
            {
                var winRate = (double)league.WinningBets / league.TotalBets;
                
                recommendations.Add(new BetRecommendationViewModel
                {
                    Type = RecommendationTypeViewModel.SportLeague,
                    SportLeagueId = league.SportLeagueId,
                    SportLeagueName = league.SportLeagueName,
                    Confidence = CalculateConfidence(winRate, league.TotalBets),
                    Reasoning = $"You've won {league.WinningBets} of {league.TotalBets} bets ({(winRate * 100):F1}%) in {league.SportLeagueName} " +
                               $"with a profit of {league.TotalProfit:C}",
                    SuggestedAction = $"Consider placing bets in {league.SportLeagueName}"
                });
            }

            // Generate recommendations based on best performing bet types
            foreach (var betType in betTypeAnalysis.Take(3))
            {
                var winRate = (double)betType.WinningBets / betType.TotalBets;
                
                recommendations.Add(new BetRecommendationViewModel
                {
                    Type = RecommendationTypeViewModel.BetType,
                    BetTypeName = betType.BetTypeName,
                    Confidence = CalculateConfidence(winRate, betType.TotalBets),
                    Reasoning = $"You've won {betType.WinningBets} of {betType.TotalBets} {betType.BetTypeName} bets ({(winRate * 100):F1}%) " +
                               $"with a profit of {betType.TotalProfit:C}",
                    SuggestedAction = $"Consider {betType.BetTypeName} bets around {betType.AverageOdds:F0} odds"
                });
            }

            // Generate recommendations based on best performing teams
            foreach (var team in teamPerformance.Take(3))
            {
                var winRate = (double)team.WinningBets / team.TotalBets;
                
                recommendations.Add(new BetRecommendationViewModel
                {
                    Type = RecommendationTypeViewModel.Team,
                    TeamName = team.Team,
                    Confidence = CalculateConfidence(winRate, team.TotalBets),
                    Reasoning = $"You've won {team.WinningBets} of {team.TotalBets} bets ({(winRate * 100):F1}%) on {team.Team} " +
                               $"with a profit of {team.TotalProfit:C}",
                    SuggestedAction = $"Consider betting on {team.Team}"
                });
            }

            // Generate suggestions for optimal bet sizes
            var averageBetSize = userBets.Average(b => b.Stake);
            var winningBetSizeAvg = userBets.Where(b => b.Result == BetResult.WIN).Average(b => b.Stake);
            
            if (winningBetSizeAvg > averageBetSize * 1.2m)
            {
                recommendations.Add(new BetRecommendationViewModel
                {
                    Type = RecommendationTypeViewModel.BetSize,
                    Confidence = 0.7,
                    Reasoning = $"Your winning bets average {winningBetSizeAvg:C}, higher than your overall average of {averageBetSize:C}",
                    SuggestedAction = $"Consider slightly increasing your bet size to around {winningBetSizeAvg:C}"
                });
            }
            else if (winningBetSizeAvg < averageBetSize * 0.8m)
            {
                recommendations.Add(new BetRecommendationViewModel
                {
                    Type = RecommendationTypeViewModel.BetSize,
                    Confidence = 0.7,
                    Reasoning = $"Your winning bets average {winningBetSizeAvg:C}, lower than your overall average of {averageBetSize:C}",
                    SuggestedAction = $"Consider slightly reducing your bet size to around {winningBetSizeAvg:C}"
                });
            }

            return recommendations.OrderByDescending(r => r.Confidence).Take(limit).ToList();
        }

        /// <summary>
        /// Calculate confidence score based on win rate and sample size
        /// </summary>
        private double CalculateConfidence(double winRate, int sampleSize)
        {
            // Base confidence on win rate
            double confidence = winRate;
            
            // Adjust for sample size (more samples = higher confidence)
            if (sampleSize < 5)
                confidence *= 0.7;
            else if (sampleSize < 10) 
                confidence *= 0.8;
            else if (sampleSize < 20)
                confidence *= 0.9;
            
            // Cap confidence at 0.95
            return Math.Min(confidence, 0.95);
        }

        /// <summary>
        /// Recommend optimal bet size based on Kelly Criterion
        /// </summary>
        public double RecommendKellyBetSize(double bankroll, double winProbability, double odds)
        {
            // Kelly formula: f* = (bp - q) / b
            // where:
            // f* = fraction of bankroll to bet
            // b = decimal odds - 1 (the potential profit if you win)
            // p = probability of winning
            // q = probability of losing (1 - p)
            
            double b = odds - 1;
            double p = winProbability;
            double q = 1 - p;
            
            double kellyFraction = (b * p - q) / b;
            
            // Cap the Kelly bet at 20% of bankroll as a safety measure
            kellyFraction = Math.Min(kellyFraction, 0.2);
            
            // Ensure we don't recommend negative bets
            kellyFraction = Math.Max(0, kellyFraction);
            
            return bankroll * kellyFraction;
        }
    }
}
