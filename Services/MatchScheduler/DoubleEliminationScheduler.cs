namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data;
    using Tournament.Data.Models;

    public enum BracketType
    {
        Winners,
        Losers,
        Championship
    }

    public class RoundX
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public BracketType Bracket { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
    }

    public interface IMatchGeneratorDbl
    {
        List<RoundX> Generate(List<Team> teams, Tournament tournament);
        //List<Round> GenerateRounds(TurnirDbContext dbContext, List<Team> teams, Tournament tournament);
    }



    public class DoubleEliminationScheduler : IMatchGenerator
    {
                private readonly TurnirDbContext _context;

        public DoubleEliminationScheduler(TurnirDbContext dbContext)
        {
            this._context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }


        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {

            if (!IsPowerOfTwo(teams.Count))
                throw new InvalidOperationException("Double Elimination форматът изисква брой отбори, който е степен на 2 (напр. 4, 8, 16).");
            // Random seeding
            var rnd = new Random();
            teams = teams.OrderBy(t => rnd.Next()).ToList();

            // Pad to next power of two
            int size = 1;
            while (size < teams.Count) size <<= 1;
            for (int i = teams.Count; i < size; i++)
                teams.Add(null);  // bye

            var rounds = new List<Match>();
            var winnersQueue = new Queue<Team>(teams);
            int totalWBRounds = (int)Math.Log(size, 2);
            var losersQueue = new Queue<Team>();

            // Winners bracket
            for (int r = 1; r <= totalWBRounds; r++)
            {
                var round = new Match
                {
                    Number = r,
                    Bracket = BracketType.Winners,
                    Name = r == totalWBRounds ? "Winners Final" : $"Winners Round {r}"
                };

                var nextWinners = new List<Team>();
                while (winnersQueue.Count >= 2)
                {
                    var a = winnersQueue.Dequeue();
                    var b = winnersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = (int)a?.Id,
                        TeamBId = (int)b?.Id,
                        Round = r,
                        Bracket = BracketType.Winners,
                        PlayedOn = tournament.StartDate.AddDays((r - 1) * 2)
                    };
                    _context.Matches.Add(match);
                    _context.SaveChanges();

                    nextWinners.Add(match.WinnerTeam);
                    if (match.LoserTeam != null)
                        losersQueue.Enqueue(match.LoserTeam);

                    round.Matches.Add(match);
                }

                winnersQueue = new Queue<Team>(nextWinners);
                rounds.Add(round);
            }

            // Losers bracket
            for (int r = 1; r <= totalWBRounds && losersQueue.Count > 1; r++)
            {
                var round = new Match
                {
                    Number = totalWBRounds + r,
                    Bracket = BracketType.Losers,
                    Name = r == totalWBRounds ? "Losers Final" : $"Losers Round {r}"
                };

                var nextLosers = new List<Team>();
                while (losersQueue.Count >= 2)
                {
                    var a = losersQueue.Dequeue();
                    var b = losersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = a.Id,
                        TeamBId = b.Id,
                        Round = round.Number,
                        Bracket = BracketType.Losers,
                        PlayedOn = tournament.StartDate.AddDays((round.Number - 1) * 2)
                    };
                    _context.Matches.Add(match);
                    _context.SaveChanges();

                    nextLosers.Add(match.WinnerTeam);
                    round.Matches.Add(match);
                }

                losersQueue = new Queue<Team>(nextLosers);
                rounds.Add(round);
            }

            // Championship
            var champRound = new Match
            {
                Number = rounds.Max(r => r.Number) + 1,
                Bracket = BracketType.Championship,
                Name = "Championship"
            };
            var wf = rounds.First(r => r.Bracket == BracketType.Winners && r.Name.Contains("Final")).Matches.First();
            var lf = rounds.First(r => r.Bracket == BracketType.Losers && r.Name.Contains("Final")).Matches.First();
            var finalMatch = new Match
            {
                TournamentId = tournament.Id,
                SourceMatchAId = wf.Id,
                SourceMatchBId = lf.Id,
                Round = champRound.Number,
                Bracket = BracketType.Championship,
                PlayedOn = tournament.StartDate.AddDays(totalWBRounds * 3)
            };
            _context.Matches.Add(finalMatch);
            _context.SaveChanges();

            champRound.Matches.Add(finalMatch);
            rounds.Add(champRound);

            return rounds;
        }

        private bool IsPowerOfTwo(int number)
        {
            return number > 1 && (number & (number - 1)) == 0;
        }


    }

}
