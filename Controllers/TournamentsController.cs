namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Services.MatchScheduler;

    public class TournamentsController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly IMatchSchedulerService _matchScheduler;

        public TournamentsController(IMatchSchedulerService matchScheduler, TurnirDbContext context)
        {
            _context = context;
            _matchScheduler = matchScheduler;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var matches = this._context.Matches.Any();
            var tournaments = await _context.Tournaments.ToListAsync();
            TempData["data"] = !matches ? "You can set items!" : "Attension! Tournament on progress!";
            return View(tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currid = _context
                .Tournaments
                .Where(t => t.IsActive)
                .FirstOrDefault();

            if (id == 0)
            {
                id = currid.Id;
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamA)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamB)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
                return NotFound();

            var totalMatchesCount = _context.Matches.Count();

            var lastUpdatedCount = _context
                .Matches
                .Where(m => m.ScoreA != null)
                .Count();

            var isFinal = _context
                .Matches
                .Where(m => m.IsFinal)
                .FirstOrDefault();

            if (totalMatchesCount == lastUpdatedCount && isFinal==null)
            {
                ViewData["IsFinal"] = "Final";
            }
            else
            {
                ViewData["IsFinal"] = "";
            }

            return View(tournament);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Tournament updated)
        {
            if (id != updated.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(updated);

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
                return NotFound();

            // Променям текущия турнир
            tournament.Name = updated.Name;
            tournament.StartDate = updated.StartDate;
            tournament.IsActive = true;// updated.IsActive;

            // Обвързваме логически: ако е активен → заявки = true, иначе false
            tournament.IsOpenForApplications = true;// updated.IsOpenForApplications;

            // Ако активираме този турнир, всички останали стават неактивни и затворени
            if (updated.IsActive)
            {
                var others = await _context.Tournaments
                    .Where(t => t.Id != updated.Id)
                    .ToListAsync();

                foreach (var t in others)
                {
                    t.IsActive = false;
                    t.IsOpenForApplications = false;
                }
                var othersTeams = await _context.Teams
                    .Where(t => t.FeePaid == true)
                    .ToListAsync();

                foreach (var t in othersTeams)
                {
                    t.TournamentId = updated.Id;
                }

                var managerReqs = _context.ManagerRequests
                    .Where(m => m.IsApproved && m.FeePaid)
                    .ToList();

                foreach (var m in managerReqs)
                {
                    m.TournamentId = updated.Id;
                    m.TournamentType = updated.Type;
                }

            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Турнирът {updated.Name} беше успешно обновен.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult SelectForSchedule()
        {
            int tournamentId = -3;

            var existingTournamentId = _context
                .Tournaments
                .Where(t => t.IsActive == true)
                .Select(t => t.Id)
                .FirstOrDefault();

            if (existingTournamentId > 0)
            {
                tournamentId = (int)existingTournamentId;
            }


            var isExistingMatches = _context.Matches.Any();

            if (isExistingMatches)
            {
                var existingMatches = _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .Where(m => m.TournamentId == tournamentId)
                    .OrderBy(m => m.PlayedOn)
                    .ToList();

                ViewBag.TournamentId = tournamentId;
                return View("ConfirmScheduleOverwrite", existingMatches);
            }

            return RedirectToAction(nameof(GenerateSchedule), new { tournamentId });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateNewSchedule(int id)
        {
            return await GenerateSchedule(id);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                TempData["Message"] = "Турнирът не съществува.";
                return RedirectToAction("Index");
            }

            // 🔴 Изтриваме всички стари мачове за този турнир
            var existingMatches = await _context.Matches
                .Where(m => m.Id > 0)
                //.Where(m => m.TournamentId == tournamentId)
                .ToListAsync();

            _context.Matches.RemoveRange(existingMatches);
            await _context.SaveChangesAsync();

            // 🟢 Зареждаме одобрените отбори
            var approvedTeams = await _context.ManagerRequests
                .Where(r => r.TournamentId == tournamentId && r.IsApproved && r.FeePaid)
                .Select(r => r.TeamId)
                .ToListAsync();

            var teams = await _context.Teams
                .Where(t => approvedTeams.Contains(t.Id))
                .ToListAsync();

            if (teams.Count < 4)
            {
                TempData["Message"] = "Няма достатъчно отбори за създаване на график.";
                return RedirectToAction("Index");
            }

            try
            {
                // 🟢 Генерираме нов график чрез MatchScheduler
                var matches = _matchScheduler.GenerateSchedule(teams, tournament,_context);
                _context.Matches.AddRange(matches);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", new { id = tournament.Id });
            }

            TempData["Message"] = "✅ Новият график беше успешно генериран.";
            return RedirectToAction("Details", new { id = tournament.Id });
            //return RedirectToAction("Index", "Matches");
        }

        // 📄 File: Controllers/TournamentsController.cs
        // 📄 File: Controllers/TournamentsController.cs
        [HttpPost]
        public async Task<IActionResult> GenerateNextKnockoutRound(int tournamentId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return NotFound();

            var allMatches = _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToList();

            var lastDate = allMatches.Any()
                ? allMatches.Max(m => m.PlayedOn)
                : tournament.StartDate;

            var latestRoundDate = allMatches.Max(m => m.PlayedOn);
            var lastRoundMatches = allMatches.Where(m => m.PlayedOn == latestRoundDate).ToList();

            var winners = new List<int>();
            foreach (var match in lastRoundMatches)
            {
                if (match.ScoreA > match.ScoreB)
                    winners.Add(match.TeamAId);
                else if (match.ScoreB > match.ScoreA)
                    winners.Add(match.TeamBId);
                else
                {
                    TempData["Message"] = $"Мач между {match.TeamA?.Name} и {match.TeamB?.Name} няма победител. Равенствата не са позволени.";
                    return RedirectToAction("Details", new { id = tournamentId });
                }
            }

            if (winners.Count == 1)
            {
                TempData["Message"] = "Турнирът приключи. Победител е: " +
                    (await _context.Teams.FindAsync(winners[0])).Name;
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var shuffled = winners.OrderBy(x => Guid.NewGuid()).ToList();
            var newMatches = new List<Match>();

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                var match = new Match
                {
                    TournamentId = tournamentId,
                    TeamAId = shuffled[i],
                    TeamBId = shuffled[i + 1],
                    PlayedOn = lastDate.Value.AddDays(7),
                    IsFinal = (shuffled.Count == 2)
                };
                newMatches.Add(match);
            }

            _context.Matches.AddRange(newMatches);
            await _context.SaveChangesAsync();

            TempData["Message"] = shuffled.Count == 2
                ? "Генериран е ФИНАЛ на турнира."
                : "Създаден е следващ елиминационен кръг.";

            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateNextDoubleEliminationRound(int tournamentId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return NotFound();

            var allMatches = _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToList();

            var latestRoundDate = allMatches.Max(m => m.PlayedOn);
            var lastRoundMatches = allMatches.Where(m => m.PlayedOn == latestRoundDate).ToList();

            var winners = new List<int>();
            foreach (var match in lastRoundMatches)
            {
                if (match.ScoreA > match.ScoreB)
                    winners.Add(match.TeamAId);
                else if (match.ScoreB > match.ScoreA)
                    winners.Add(match.TeamBId);
                else
                {
                    TempData["Message"] = $"Мач между {match.TeamA?.Name} и {match.TeamB?.Name} няма победител. Равенствата не са позволени.";
                    return RedirectToAction("Details", new { id = tournamentId });
                }
            }

            if (winners.Count == 1)
            {
                TempData["Message"] = "Турнирът приключи. Победител е: " +
                    (await _context.Teams.FindAsync(winners[0])).Name;
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var shuffled = winners.OrderBy(x => Guid.NewGuid()).ToList();
            var newMatches = new List<Match>();

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                var match = new Match
                {
                    TournamentId = tournamentId,
                    TeamAId = shuffled[i],
                    TeamBId = shuffled[i + 1],
                    PlayedOn = latestRoundDate.Value.AddDays(7),
                    IsFinal = (shuffled.Count == 2)
                };
                newMatches.Add(match);
            }

            _context.Matches.AddRange(newMatches);
            await _context.SaveChangesAsync();

            TempData["Message"] = shuffled.Count == 2
                ? "Генериран е ФИНАЛ на турнира."
                : "Създаден е следващ елиминационен кръг.";

            return RedirectToAction("Details", new { id = tournamentId });
        }


        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateFinal(int tournamentId)
        {
            var matches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .OrderBy(m => m.PlayedOn)
                .ToListAsync();

            if (matches.Count < 2)
            {
                TempData["Message"] = "Не са налични достатъчно полуфинали за създаване на финал.";
                return RedirectToAction("Index");
            }

            // 🔎 Проверка дали вече има финал
            if (matches.Any(m => m.IsFinal))
            {
                TempData["Message"] = "⚠️ Финалът вече съществува.";
                return RedirectToAction("Index");
            }

            var semi1 = matches[0];
            var semi2 = matches[1];

            // Проверка дали има въведени резултати
            if (semi1.ScoreA == null || semi1.ScoreB == null || semi2.ScoreA == null || semi2.ScoreB == null)
            {
                TempData["Message"] = "Трябва първо да бъдат въведени резултатите от полуфиналите.";
                return RedirectToAction("Index");
            }

            // Определяме победителите
            var winner1Id = semi1.ScoreA > semi1.ScoreB ? semi1.TeamAId : semi1.TeamBId;
            var winner2Id = semi2.ScoreA > semi2.ScoreB ? semi2.TeamAId : semi2.TeamBId;

            // Най-късната дата
            var maxPlayedOn = matches.Max(m => m.PlayedOn) ?? DateTime.Now;

            // Създаваме финалния мач
            var finalMatch = new Match
            {
                TeamAId = winner1Id,
                TeamBId = winner2Id,
                TournamentId = tournamentId,
                PlayedOn = maxPlayedOn.AddDays(7),
                IsFinal = true
            };

            _context.Matches.Add(finalMatch);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Финалът беше успешно създаден!";
            return RedirectToAction("Index");
        }



        private List<List<(Team Home, Team Away)>> GenerateRoundRobin(List<Team> teams, bool reverseHomeAway = false)
        {
            var rounds = new List<List<(Team, Team)>>();

            var teamCount = teams.Count;
            var teamList = new List<Team>(teams);

            if (teamCount % 2 != 0)
            {
                teamList.Add(null);
                teamCount++;
            }

            int totalRounds = teamCount - 1;
            int matchesPerRound = teamCount / 2;

            for (int round = 0; round < totalRounds; round++)
            {
                var roundMatches = new List<(Team, Team)>();

                for (int match = 0; match < matchesPerRound; match++)
                {
                    var home = teamList[match];
                    var away = teamList[teamCount - 1 - match];

                    if (home != null && away != null)
                    {
                        roundMatches.Add(reverseHomeAway ? (away, home) : (home, away));
                    }
                }

                var last = teamList[teamCount - 1];
                teamList.RemoveAt(teamCount - 1);
                teamList.Insert(1, last);

                rounds.Add(roundMatches);
            }

            return rounds;
        }
    }
}