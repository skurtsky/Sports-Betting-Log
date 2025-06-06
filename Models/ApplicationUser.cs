using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace SportsBettingTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom properties
        public string DisplayName { get; set; } = string.Empty;
        public bool IsDemoUser { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Social & Privacy settings
        public bool IsProfilePublic { get; set; } = true; // By default, profiles are public
        public bool DefaultBetPrivacy { get; set; } = true; // By default, bets are public
        
        // Navigation properties for social features
        public virtual ICollection<UserFollow> FollowedByUsers { get; set; } = new List<UserFollow>();
        public virtual ICollection<UserFollow> FollowingUsers { get; set; } = new List<UserFollow>();
    }
}
