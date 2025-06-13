using System.ComponentModel.DataAnnotations;

namespace SportsBettingTracker.Models
{    public class SportLeague
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        // Navigation property to related bets
        public ICollection<Bet>? Bets { get; set; }
    }
}
