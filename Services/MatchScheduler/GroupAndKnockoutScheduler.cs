namespace Tournament.Services.MatchScheduler
{
    using Tournament.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data;

    public class GroupAndKnockoutScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament,TurnirDbContext c=null)
        {
            var matches = new List<Match>();

            if (teams.Count < 4)
                throw new InvalidOperationException("Group + Knockout форматът изисква поне 4 отбора.");

            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();
            var startDate = tournament.StartDate;

            // Разделяме на 2 групи
            var groupA = shuffled.Take(shuffled.Count / 2).ToList();
            var groupB = shuffled.Skip(shuffled.Count / 2).ToList();

            // Мачове в група A (всеки срещу всеки)
            for (int i = 0; i < groupA.Count; i++)
            {
                for (int j = i + 1; j < groupA.Count; j++)
                {
                    matches.Add(new Match
                    {
                        TeamAId = groupA[i].Id,
                        TeamBId = groupA[j].Id,
                        TournamentId = tournament.Id,
                        PlayedOn = startDate.AddDays(i + j)
                    });
                }
            }

            // Мачове в група B (всеки срещу всеки)
            for (int i = 0; i < groupB.Count; i++)
            {
                for (int j = i + 1; j < groupB.Count; j++)
                {
                    matches.Add(new Match
                    {
                        TeamAId = groupB[i].Id,
                        TeamBId = groupB[j].Id,
                        TournamentId = tournament.Id,
                        PlayedOn = startDate.AddDays(7 + i + j) // малко по-късно
                    });
                }
            }

            // Полуфинали ще се добавят ръчно след изиграване на груповите срещи
            return matches;
        }
    }
}

