using SportsBettingTracker.Models;
using System.Collections.Generic;

namespace SportsBettingTracker.ViewModels
{
    public class DashboardViewModel
    {
        public string DateRange { get; set; } = "YTD";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBets { get; set; }
        public int TotalWins { get; set; }
        public List<BetRecommendationViewModel> TopRecommendations { get; set; } = new List<BetRecommendationViewModel>();
        public int TotalLosses { get; set; }
        public int TotalPushes { get; set; }
        public decimal NetProfit { get; set; }
        public decimal TotalStake { get; set; }
        public decimal WinPercentage { get; set; }
        public decimal ROI { get; set; }
        public int CurrentStreak { get; set; }
        public string StreakType { get; set; } = "";        public int LongestWinStreak { get; set; }
        public int LongestLossStreak { get; set; }
        public List<ProfitBySport> ProfitBySport { get; set; } = new List<ProfitBySport>();
        public List<ProfitByBetType> ProfitByBetType { get; set; } = new List<ProfitByBetType>();
        public IEnumerable<Bet> PendingBets { get; set; } = new List<Bet>();
        
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartData { get; set; } = new List<decimal>();
        public List<decimal> CumulativeChartData { get; set; } = new List<decimal>();
        public string FormattedNetProfit => 
            NetProfit >= 0 ? $"+${NetProfit:F2}" : $"-${Math.Abs(NetProfit):F2}";
            
        public string FormattedWinPercentage => $"{WinPercentage:F1}%";
        
        public string FormattedROI => $"{ROI:F1}%";
        
        public string FormattedStreak => 
            CurrentStreak > 0 ? $"{CurrentStreak} {StreakType}" : "No current streak";
            
        public string FormattedLongestWinStreak => $"{LongestWinStreak} wins";
        
        public string FormattedLongestLossStreak => $"{LongestLossStreak} losses";
        
        public Dictionary<string, string> AvailableDateRanges => new()
        {
            { "Last7Days", "Last 7 Days" },
            { "Last30Days", "Last 30 Days" },
            { "Last90Days", "Last 90 Days" },
            { "Last6Months", "Last 6 Months" },
            { "YTD", "Year to Date" },
            { "AllTime", "All Time" }
        };
    }
}
