using SportsBettingTracker.Models;
using System.Collections.Generic;

namespace SportsBettingTracker.ViewModels
{
    public class ProfileViewModel
    {        public required ApplicationUser User { get; set; }
        public required List<Bet> RecentBets { get; set; }
        public int TotalBets { get; set; }
        public double WinRate { get; set; }
        public decimal NetProfit { get; set; }
        public double ROI { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsOwnProfile { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        
        public string FormattedWinRate => $"{WinRate:F1}%";
        public string FormattedROI => $"{ROI:F1}%";
        public string FormattedNetProfit => NetProfit >= 0 
            ? $"+${NetProfit:F2}" 
            : $"-${System.Math.Abs(NetProfit):F2}";
    }
}
