using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsBettingTracker.Models
{
    public enum BetResult
    {
        WIN,
        LOSS,
        PUSH,
        PENDING
    }

    public class Bet
    {
        public int Id { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Bet Date")]
        public DateTime BetDate { get; set; }
        
        [Required]
        public int SportLeagueId { get; set; }
        
        [ForeignKey("SportLeagueId")]
        [Display(Name = "Sport/League")]
        public SportLeague? SportLeague { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Match")]
        public string Match { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Bet Selection")]
        public string BetSelection { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Stake ($)")]
        public decimal Stake { get; set; }
        
        [Required]
        [Display(Name = "Odds")]
        public int Odds { get; set; }
        
        [Display(Name = "Result")]
        public BetResult Result { get; set; } = BetResult.PENDING;
        
        [Display(Name = "Amount Won/Lost")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? AmountWonLost { get; set; }
        
        [NotMapped]
        public string FormattedOdds => Odds > 0 ? $"+{Odds}" : $"{Odds}";
        
        [NotMapped]
        public string FormattedAmountWonLost 
        { 
            get 
            {
                if (!AmountWonLost.HasValue)
                    return "Pending";
                    
                return AmountWonLost.Value >= 0 
                    ? $"+${AmountWonLost.Value:F2}" 
                    : $"-${Math.Abs(AmountWonLost.Value):F2}";
            }
        }
        
        public void CalculateWinLoss()
        {
            if (Result == BetResult.PUSH)
            {
                AmountWonLost = 0;
                return;
            }
            
            if (Result == BetResult.PENDING)
            {
                AmountWonLost = null;
                return;
            }
            
            if (Result == BetResult.WIN)
            {
                // Calculate winnings based on American odds
                if (Odds > 0)
                {
                    // Positive odds (e.g. +150) means you win $150 on a $100 bet
                    AmountWonLost = Stake * (decimal)Odds / 100;
                }
                else
                {
                    // Negative odds (e.g. -110) means you need to bet $110 to win $100
                    AmountWonLost = Stake * 100 / Math.Abs((decimal)Odds);
                }
            }
            else // LOSS
            {
                AmountWonLost = -Stake;
            }
        }
    }
}
