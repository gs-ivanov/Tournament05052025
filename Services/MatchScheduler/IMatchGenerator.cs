namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using Tournament.Data;
    using Tournament.Data.Models;

    public interface IMatchGenerator
    {
        List<Match> Generate(List<Team> teams, Tournament tournament, TurnirDbContext c);
    }
}
