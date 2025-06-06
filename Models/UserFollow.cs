using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsBettingTracker.Models
{
    public class UserFollow
    {
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; } = string.Empty;

        [Required]
        public string FollowingId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual ApplicationUser? Follower { get; set; }

        [ForeignKey("FollowingId")]
        public virtual ApplicationUser? Following { get; set; }
    }
}
