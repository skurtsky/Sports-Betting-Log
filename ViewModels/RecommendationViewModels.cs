using System;
using System.Collections.Generic;

namespace SportsBettingTracker.ViewModels
{
    public class BetRecommendationViewModel
    {
        public RecommendationTypeViewModel Type { get; set; }
        public int? SportLeagueId { get; set; }
        public string? SportLeagueName { get; set; }
        public string? BetTypeName { get; set; }
        public string? TeamName { get; set; }
        public double Confidence { get; set; } // 0.0 to 1.0
        public string Reasoning { get; set; } = string.Empty;
        public string SuggestedAction { get; set; } = string.Empty;
    }

    public enum RecommendationTypeViewModel
    {
        SportLeague,
        BetType,
        Team,
        BetSize
    }

    public class BetCalculatorViewModel
    {
        public double CurrentBankroll { get; set; }
        public double EstimatedWinPercentage { get; set; }
        public double DecimalOdds { get; set; }
        public double RecommendedBetAmount { get; set; }
        public bool CalculationComplete { get; set; } = false;
    }
}
