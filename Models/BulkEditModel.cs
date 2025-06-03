using System;
using System.Collections.Generic;

namespace SportsBettingTracker.Models
{    public class BulkEditModel
    {
        public int[]? Ids { get; set; }
        public string? Result { get; set; }
        public string? SportLeagueId { get; set; }
        public string? BetType { get; set; }
    }
}
