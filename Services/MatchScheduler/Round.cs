namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using Tournament.Data.Models;

    public class Round
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public BracketType Bracket { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
    }
}
