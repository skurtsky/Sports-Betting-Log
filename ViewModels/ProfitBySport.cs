namespace SportsBettingTracker.ViewModels
{
    public class ProfitBySport
    {
        public string SportLeagueName { get; set; } = string.Empty;
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal NetProfit { get; set; }
        public decimal WinPercentage { get; set; }
        
        public string FormattedNetProfit => 
            NetProfit >= 0 ? $"+${NetProfit:F2}" : $"-${Math.Abs(NetProfit):F2}";
        
        public string FormattedWinPercentage => $"{WinPercentage:F1}%";
    }
}
