namespace Tournament.Services.MatchScheduler
{
    using Tournament.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data;

    public class SwissScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament,TurnirDbContext c)
        {
            var matches = new List<Match>();

            if (teams.Count < 4)
                throw new InvalidOperationException("Swiss форматът изисква поне 4 отбора.");

            var shuffledTeams = teams.OrderBy(t => Guid.NewGuid()).ToList();
            var startDate = tournament.StartDate;

            // Кръг 1
            matches.Add(new Match
            {
                TeamAId = shuffledTeams[0].Id,
                TeamBId = shuffledTeams[1].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate
            });

            matches.Add(new Match
            {
                TeamAId = shuffledTeams[2].Id,
                TeamBId = shuffledTeams[3].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate
            });

            // Кръг 2 (кръстосване)
            matches.Add(new Match
            {
                TeamAId = shuffledTeams[0].Id,
                TeamBId = shuffledTeams[2].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate.AddDays(7)
            });

            matches.Add(new Match
            {
                TeamAId = shuffledTeams[1].Id,
                TeamBId = shuffledTeams[3].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate.AddDays(7)
            });

            // Кръг 3 (други двойки)
            matches.Add(new Match
            {
                TeamAId = shuffledTeams[0].Id,
                TeamBId = shuffledTeams[3].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate.AddDays(14)
            });

            matches.Add(new Match
            {
                TeamAId = shuffledTeams[1].Id,
                TeamBId = shuffledTeams[2].Id,
                TournamentId = tournament.Id,
                PlayedOn = startDate.AddDays(14)
            });

            return matches;
        }
    }
}


