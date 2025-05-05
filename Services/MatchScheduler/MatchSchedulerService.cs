namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using System;
    using Tournament.Models;
    using Tournament.Data.Models;
    using Tournament.Data;

    public class MatchSchedulerService : IMatchSchedulerService
    {
        private TurnirDbContext _dbContext;
        public MatchSchedulerService(TurnirDbContext dbContext)
        {
                this._dbContext = dbContext;
        }
        public List<Match> GenerateSchedule(List<Team> teams, Tournament tournament)
        {
            IMatchGenerator generator = tournament.Type switch
            {
                TournamentType.RoundRobin => new RoundRobinScheduler(),
                TournamentType.Knockout => new KnockoutScheduler(),
                TournamentType.DoubleElimination =>new DoubleEliminationScheduler(this._dbContext),
                TournamentType.GroupAndKnockout => new GroupAndKnockoutScheduler(),
                TournamentType.Swiss => new SwissScheduler(),
                _ => throw new NotSupportedException("Типът турнир не се поддържа.")
            };
            //**
            return generator.Generate(teams, tournament);
        }

    }
}
