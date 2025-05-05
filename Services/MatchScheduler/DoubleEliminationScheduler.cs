namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data;
    using Tournament.Data.Models;

    /// <summary>
    /// Specifies the type of bracket for a round.
    /// </summary>
    public enum BracketType
    {
        Winners,
        Losers,
        Championship
    }
    /// Represents a single tournament round with its matches.
    /// </summary>
    public class Round
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public BracketType Bracket { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
    }
    /// Generates rounds for a double-elimination bracket without requiring DbContext injection.
    /// </summary>
    public interface IMatchGeneratorDbl
    {
        List<Round> Generate(List<Team> teams, Tournament tournament, TurnirDbContext dbContext);
    }



    public class DoubleEliminationScheduler : IMatchGenerator
    {
        //private readonly TurnirDbContext _context;

        //public DoubleEliminationScheduler(TurnirDbContext dbContext)
        //{
        //    this._context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        //}


        public List<Round> Generate(List<Team> teams, Tournament tournament, TurnirDbContext dbContext)
        {
            // Clear existing matches for this tournament
            var existing = dbContext.Matches.Where(m => m.TournamentId == tournament.Id);
            dbContext.Matches.RemoveRange(existing);
            dbContext.SaveChanges();



            if (teams == null || teams.Count < 2)
                throw new ArgumentException("At least two teams are required.");

            if (!IsPowerOfTwo(teams.Count))
                throw new InvalidOperationException("Double Elimination requires the number of teams to be a power of two.");
            // Random seeding
            var rnd = new Random();
            teams = teams.OrderBy(t => rnd.Next()).ToList();

            // Pad with null (byes) to power of two
            int size = 1;
            while (size < teams.Count) size <<= 1;
            for (int i = teams.Count; i < size; i++) teams.Add(null);

            var rounds = new List<Round>();
            var winnersQueue = new Queue<Team>(teams);
            var losersQueue = new Queue<Team>();
            int totalWBRounds = (int)Math.Log(size, 2);

            // ===== Winners Bracket =====
            for (int r = 1; r <= totalWBRounds; r++)
            {
                var round = new Round
                {
                    Number = r,
                    Bracket = BracketType.Winners,
                    Name = r == totalWBRounds ? "Winners Final" : $"Winners Round {r}"
                };

                var nextWinners = new List<Team>();
                while (winnersQueue.Count >= 2)
                {
                    var teamA = winnersQueue.Dequeue();
                    var teamB = winnersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = (int)teamA?.Id,
                        TeamBId = (int)teamB?.Id,
                        Round = round.Number,
                        Bracket = round.Bracket,
                        PlayedOn = tournament.StartDate.AddDays((r - 1) * 2)
                    };
                    dbContext.Matches.Add(match);
                    dbContext.SaveChanges();

                    nextWinners.Add(match.WinnerTeam);
                    if (match.LoserTeam != null)
                        losersQueue.Enqueue(match.LoserTeam);

                    round.Matches.Add(match);
                }

                winnersQueue = new Queue<Team>(nextWinners);
                rounds.Add(round);
            }

            // ===== Losers Bracket =====
            for (int r = 1; r <= totalWBRounds && losersQueue.Count > 1; r++)
            {
                var round = new Round
                {
                    Number = totalWBRounds + r,
                    Bracket = BracketType.Losers,
                    Name = r == totalWBRounds ? "Losers Final" : $"Losers Round {r}"
                };

                var nextLosers = new List<Team>();
                while (losersQueue.Count >= 2)
                {
                    var teamA = losersQueue.Dequeue();
                    var teamB = losersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = teamA.Id,
                        TeamBId = teamB.Id,
                        Round = round.Number,
                        Bracket = round.Bracket,
                        PlayedOn = tournament.StartDate.AddDays((round.Number - 1) * 2)
                    };
                    dbContext.Matches.Add(match);
                    dbContext.SaveChanges();

                    nextLosers.Add(match.WinnerTeam);
                    round.Matches.Add(match);
                }

                losersQueue = new Queue<Team>(nextLosers);
                rounds.Add(round);
            }

            // ===== Championship =====
            var champRound = new Round
            {
                Number = rounds.Max(r => r.Number) + 1,
                Bracket = BracketType.Championship,
                Name = "Championship"
            };
            var winnersFinal = rounds.First(r => r.Bracket == BracketType.Winners && r.Matches.Any()).Matches.Last();
            var losersFinal = rounds.First(r => r.Bracket == BracketType.Losers && r.Matches.Any()).Matches.Last();
            var finalMatch = new Match
            {
                TournamentId = tournament.Id,
                TeamAId = (int)winnersFinal.WinnerTeam?.Id,
                TeamBId = (int)losersFinal.WinnerTeam?.Id,
                Round = champRound.Number,
                Bracket = champRound.Bracket,
                PlayedOn = tournament.StartDate.AddDays(totalWBRounds * 3)
            };
            dbContext.Matches.Add(finalMatch);
            dbContext.SaveChanges();

            champRound.Matches.Add(finalMatch);
            rounds.Add(champRound);

            return rounds;
        }

        List<Match> IMatchGenerator.Generate(List<Team> teams, Tournament tournament, TurnirDbContext c)
        {
            throw new NotImplementedException();
        }

        private bool IsPowerOfTwo(int number)
        {
            return number > 1 && (number & (number - 1)) == 0;
        }


    }

}
