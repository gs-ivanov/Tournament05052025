namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data.Models;

    public class RoundRobinScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {
            var matches = new List<Match>();

            if (teams.Count < 2)
                throw new ArgumentException("Необходими са поне 2 отбора за Round-Robin график.");

            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();
            int numRounds = shuffled.Count - 1;
            int matchesPerRound = shuffled.Count / 2;

            // Добавяме фиктивен отбор ако броят е нечетен
            if (shuffled.Count % 2 != 0)
            {
                shuffled.Add(null);
                numRounds++;
            }

            for (int round = 0; round < numRounds; round++)
            {
                for (int i = 0; i < matchesPerRound; i++)
                {
                    var teamA = shuffled[i];
                    var teamB = shuffled[shuffled.Count - 1 - i];

                    if (teamA != null && teamB != null)
                    {
                        matches.Add(new Match
                        {
                            TeamA = teamA,
                            TeamB = teamB,
                            PlayedOn = tournament.StartDate.AddDays(round * 7),
                            TournamentId = tournament.Id
                        });
                    }
                }

                // Завъртане на отборите (кръгов принцип)
                var last = shuffled.Last();
                shuffled.RemoveAt(shuffled.Count - 1);
                shuffled.Insert(1, last);
            }

            return matches;
        }
    }
}

