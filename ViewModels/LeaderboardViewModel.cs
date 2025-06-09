using SportsBettingTracker.Models;

namespace SportsBettingTracker.ViewModels
{
    public class LeaderboardViewModel
    {
        public required ApplicationUser User { get; set; }
        public int TotalBets { get; set; }
        public double WinRate { get; set; }
        public decimal NetProfit { get; set; }
        public double ROI { get; set; }
        public int FollowersCount { get; set; }

        // Formatted properties for display
        public string FormattedWinRate => $"{WinRate:F1}%";
        public string FormattedROI => $"{ROI:F1}%";
        public string FormattedNetProfit => NetProfit >= 0 
            ? $"+${NetProfit:F2}" 
            : $"-${System.Math.Abs(NetProfit):F2}";
        
        // Rank property (will be set in the view)
        public int? Rank { get; set; }
    }
}
