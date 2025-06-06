using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsBettingTracker.Models
{
    public enum NotificationType
    {
        NewFollower,
        BetLiked,
        BetCommented
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? ActorUserId { get; set; }
        
        public int? BetId { get; set; }
        
        public int? CommentId { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        [ForeignKey("ActorUserId")]
        public virtual ApplicationUser? ActorUser { get; set; }
        
        [ForeignKey("BetId")]
        public virtual Bet? Bet { get; set; }
        
        [ForeignKey("CommentId")]
        public virtual BetComment? Comment { get; set; }
    }
}
