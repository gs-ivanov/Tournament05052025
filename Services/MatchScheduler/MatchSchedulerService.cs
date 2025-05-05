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
        public List<Match> GenerateSchedule(List<Team> teams, Tournament tournament, TurnirDbContext _context)
        {
            IMatchGenerator generator = tournament.Type switch
            {
                TournamentType.RoundRobin => new RoundRobinScheduler(),
                TournamentType.Knockout => new KnockoutScheduler(),
                TournamentType.DoubleElimination => new KnockoutScheduler(),  //DoubleEliminationScheduler(),
                TournamentType.GroupAndKnockout => new GroupAndKnockoutScheduler(),
                TournamentType.Swiss => new SwissScheduler(),
                _ => throw new NotSupportedException("Типът турнир не се поддържа.")
            }; ; ;

            if (TournamentType.DoubleElimination == tournament.Type)
            {

                return generator.Generate(teams, tournament, this._dbContext = null);
            }
            else
            {
                return generator.Generate(teams, tournament, this._dbContext = null);

            }
        }

    }
}
