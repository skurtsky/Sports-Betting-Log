using SportsBettingTracker.Models;

namespace SportsBettingTracker.ViewModels
{
    public class ProfitByBetType
    {
        public BetType BetType { get; set; }
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal NetProfit { get; set; }
        public decimal WinPercentage { get; set; }
        
        public string FormattedWinPercentage => $"{WinPercentage:F1}%";
        
        public string FormattedNetProfit => 
            NetProfit >= 0 ? $"+${NetProfit:F2}" : $"-${Math.Abs(NetProfit):F2}";
    }
}
