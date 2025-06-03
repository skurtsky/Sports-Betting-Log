using SportsBettingTracker.Models;
using System;
using System.Collections.Generic;

namespace SportsBettingTracker.ViewModels
{
    public class CalendarViewModel
    {
        public DateTime CurrentMonth { get; set; }
        public DateTime PreviousMonth => CurrentMonth.AddMonths(-1);
        public DateTime NextMonth => CurrentMonth.AddMonths(1);
        public Dictionary<DateTime, List<CalendarBetViewModel>> BetsByDate { get; set; } = new Dictionary<DateTime, List<CalendarBetViewModel>>();
        public decimal TotalMonthProfit { get; set; }
        public int TotalMonthBets { get; set; }
        public int WinningDays { get; set; }
        public int LosingDays { get; set; }
        public string FormattedMonthProfit => TotalMonthProfit.ToString("C");
        public string MonthName => CurrentMonth.ToString("MMMM yyyy");
    }

    public class CalendarBetViewModel
    {
        public int Id { get; set; }
        public string Match { get; set; } = string.Empty;
        public string BetSelection { get; set; } = string.Empty;
        public BetResult Result { get; set; }
        public string SportLeagueName { get; set; } = string.Empty;
        public decimal Stake { get; set; }
        public decimal? AmountWonLost { get; set; }
        public string FormattedAmountWonLost => AmountWonLost.HasValue ? AmountWonLost.Value.ToString("C") : "-";
        public string ResultClass => Result == BetResult.WIN ? "win" : 
                                    Result == BetResult.LOSS ? "loss" : 
                                    Result == BetResult.PUSH ? "push" : "pending";
    }
}
