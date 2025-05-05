namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data;
    using Tournament.Data.Models;

    // 📄 File: Services/MatchScheduler/KnockoutScheduler.cs
    public class KnockoutScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament,TurnirDbContext c=null)
        {
            if (!IsPowerOfTwo(teams.Count))
                throw new InvalidOperationException("Knockout форматът изисква брой отбори, който е степен на 2 (напр. 4, 8, 16).");

            var matches = new List<Match>();
            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();
            DateTime roundDate = tournament.StartDate;

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                matches.Add(new Match
                {
                    TeamAId = shuffled[i].Id,
                    TeamBId = shuffled[i + 1].Id,
                    TournamentId = tournament.Id,
                    PlayedOn = roundDate
                });
            }

            return matches;
        }

        private bool IsPowerOfTwo(int number)
        {
            return number > 1 && (number & (number - 1)) == 0;
        }
    }
}
