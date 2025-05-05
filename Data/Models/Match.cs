namespace Tournament.Data.Models
{
    using global::Tournament.Services.MatchScheduler;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamAId { get; set; }                  // ✅ Ясен FK
        public Team TeamA { get; set; }

        public int TeamBId { get; set; }                  // ✅ Ясен FK
        public Team TeamB { get; set; }

        [Range(0, 100)]
        public int? ScoreA { get; set; }

        [Range(0, 100)]
        public int? ScoreB { get; set; }

        public DateTime? PlayedOn { get; set; }
        public bool IsPostponed { get; set; } = false;

        public bool IsFinal { get; set; } = false;

        public int? SourceMatchAId { get; set; }

        [NotMapped]
        [ForeignKey(nameof(SourceMatchAId))]
        public Match SourceMatchA { get; set; }

        public int? SourceMatchBId { get; set; }

        [NotMapped]
        [ForeignKey(nameof(SourceMatchBId))]
        public Match SourceMatchB { get; set; }

        public int Round { get; set; } // Номер на кръга
        public Services.MatchScheduler.BracketType Bracket { get; set; } // "Winners" или "Losers"

        public int Number { get; set; }
        public string Name { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();


        [NotMapped]
        public Team WinnerTeam
        {
            get
            {
                if (!ScoreA.HasValue || !ScoreB.HasValue)
                    return null;
                return ScoreA > ScoreB ? TeamA
                     : ScoreB > ScoreA ? TeamB
                     : null; // при равенство — може да върнете null или специален случай
            }
        }

        [NotMapped]
        public Team LoserTeam
        {
            get
            {
                if (!ScoreA.HasValue || !ScoreB.HasValue)
                    return null;
                return ScoreA < ScoreB ? TeamA
                     : ScoreB < ScoreA ? TeamB
                     : null;
            }
        }
    }
}