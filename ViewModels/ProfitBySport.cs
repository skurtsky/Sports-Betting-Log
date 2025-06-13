namespace SportsBettingTracker.ViewModels
{
    public class ProfitBySport
    {
        public string SportLeagueName { get; set; } = string.Empty;
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal NetProfit { get; set; }
        public decimal WinPercentage { get; set; }
        public decimal MedianBet { get; set; }
        public decimal MedianOdds { get; set; }
        
        public string FormattedNetProfit => 
            NetProfit >= 0 ? $"+${NetProfit:F2}" : $"-${Math.Abs(NetProfit):F2}";
        
        public string FormattedWinPercentage => $"{WinPercentage:F1}%";
        
        public string FormattedMedianBet => $"${MedianBet:F2}";
        
        public string FormattedMedianOdds {
            get {
                // Convert from decimal odds back to American odds for display
                if (MedianOdds >= 2.00m)
                {
                    int americanOdds = (int)((MedianOdds - 1.00m) * 100.00m);
                    return $"+{americanOdds}";
                }
                else if (MedianOdds > 1.00m)
                {
                    int americanOdds = (int)(-100.00m / (MedianOdds - 1.00m));
                    return $"{americanOdds}";
                }
                return "0";
            }
        }
    }
}