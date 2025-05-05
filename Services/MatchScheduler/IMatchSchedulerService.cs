namespace Tournament.Services.MatchScheduler
{
    using Tournament.Data.Models;
    using System.Collections.Generic;
    using Tournament.Data;

    public interface IMatchSchedulerService
    {
        List<Match> GenerateSchedule(List<Team> teams, Tournament tournament, TurnirDbContext _context);
    }
}
