using Microsoft.AspNetCore.Identity;
using System;

namespace SportsBettingTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom properties
        public string DisplayName { get; set; } = string.Empty;
        public bool IsDemoUser { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
