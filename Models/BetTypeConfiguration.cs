using System.ComponentModel.DataAnnotations;

namespace SportsBettingTracker.Models
{
    public class BetTypeConfiguration
    {
        public int Id { get; set; }
        
        [Required]
        public BetType BetType { get; set; }
        
        [Required]
        [Display(Name = "Display Name")]
        [StringLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
