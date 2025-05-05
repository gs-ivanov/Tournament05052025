namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data.Models;

    public class KnockoutScheduler_Old : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {
            var matches = new List<Match>();

            if (!(teams.Count == 4 || teams.Count == 8 || teams.Count == 16))
                throw new InvalidOperationException("Knockout форматът изисква точно 4, 8 или 16 отбора за изпълнение на турнира.");

            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();

            // Полуфинали
            Match semi1 = null;
            Match semi2 = null;
            Match semi3 = null;
            Match semi4 = null;

            if (shuffled.Count()==4)
            {
                 semi1 = new Match
                {
                    TeamA = shuffled[0],
                    TeamB = shuffled[1],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate
                };
                 semi2 = new Match
                {
                    TeamA = shuffled[2],
                    TeamB = shuffled[3],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(2)
                };

                matches.Add(semi1);
                matches.Add(semi2);
            }
            else if (shuffled.Count() == 8)
            {
                 semi1 = new Match
                {
                    TeamA = shuffled[0],
                    TeamB = shuffled[1],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate
                };
                 semi2 = new Match
                {
                    TeamA = shuffled[2],
                    TeamB = shuffled[3],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(2)
                };


                 semi3 = new Match
                {
                    TeamA = shuffled[4],
                    TeamB = shuffled[5],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(3)
                };

                 semi4 = new Match
                {
                    TeamA = shuffled[6],
                    TeamB = shuffled[7],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(4)
                };

                matches.Add(semi1);
                matches.Add(semi2);
                matches.Add(semi3);
                matches.Add(semi4);

            }
            else if (shuffled.Count() == 16)
            {
                 semi1 = new Match
                {
                    TeamA = shuffled[0],
                    TeamB = shuffled[1],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate
                };
                 semi2 = new Match
                {
                    TeamA = shuffled[2],
                    TeamB = shuffled[3],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(2)
                };


                 semi3 = new Match
                {
                    TeamA = shuffled[4],
                    TeamB = shuffled[5],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(3)
                };

                 semi4 = new Match
                {
                    TeamA = shuffled[6],
                    TeamB = shuffled[7],
                    TournamentId = tournament.Id,
                    PlayedOn = tournament.StartDate.AddDays(4)
                };
                matches.Add(semi1);
                matches.Add(semi2);
                matches.Add(semi3);
                matches.Add(semi4);
            }


            // Финал (ще се добави ръчно след резултатите от полуфиналите)
            return matches;
        }
    }
}

