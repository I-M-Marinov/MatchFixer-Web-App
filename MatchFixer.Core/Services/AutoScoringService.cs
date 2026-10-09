using MatchFixer.Common.Enums;
using MatchFixer.Common.ServiceConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchResults;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class AutoScoringService : IAutoScoringService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IFootballApiService _footballApi;
		private readonly ILiveMatchResultService _liveMatchResultService;
		private readonly ITimezoneService _timezoneService;
		private readonly ISessionService _sessionService;

		public AutoScoringService(
			MatchFixerDbContext dbContext,
			IFootballApiService footballApi,
			ILiveMatchResultService liveMatchResultService,
			ITimezoneService timezoneService,
			ISessionService sessionService)
		{
			_dbContext = dbContext;
			_footballApi = footballApi;
			_liveMatchResultService = liveMatchResultService;
			_timezoneService = timezoneService;
			_sessionService = sessionService;
		}

		public async Task<AutoScorePreviewViewModel> GetAutoScoreCandidatesAsync()
		{
			var vm = new AutoScorePreviewViewModel();
			var now = DateTime.UtcNow;

			// Only API-linked matches that have kicked off and still need a result.
			var candidates = await _dbContext.MatchEvents
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Where(m => m.ApiFixtureId != null)
				.Where(m => m.LiveResult == null)
				.Where(m => m.Status != MatchStatus.Cancelled)
				.Where(m => m.MatchDate == null || m.MatchDate <= now)
				.OrderBy(m => m.MatchDate)
				.ToListAsync();

			if (candidates.Count == 0)
			{
				vm.Message = "No API-linked matches are waiting for results right now.";
				return vm;
			}

			var fixtureIds = candidates.Select(m => m.ApiFixtureId!.Value).ToList();
			var apiResults = await _footballApi.GetFixtureResultsAsync(fixtureIds);
			var byId = apiResults.ToDictionary(r => r.ApiFixtureId);

			var timeZoneId = _sessionService.GetUserTimezone();

			foreach (var m in candidates)
			{
				var row = new AutoScoreRowViewModel
				{
					MatchId = m.Id,
					ApiFixtureId = m.ApiFixtureId!.Value,
					HomeTeam = m.HomeTeam.Name,
					AwayTeam = m.AwayTeam.Name,
					HomeLogo = string.IsNullOrWhiteSpace(m.HomeTeam.LocalLogoUrl) ? m.HomeTeam.LogoUrl : m.HomeTeam.LocalLogoUrl,
					AwayLogo = string.IsNullOrWhiteSpace(m.AwayTeam.LocalLogoUrl) ? m.AwayTeam.LogoUrl : m.AwayTeam.LocalLogoUrl,
					KickoffUtc = m.MatchDate,
					DisplayTime = _timezoneService.FormatForUserBets(m.MatchDate, timeZoneId)
				};

				if (!byId.TryGetValue(m.ApiFixtureId!.Value, out var api))
				{
					row.State = AutoScoreState.NoData;
					row.StatusShort = "-";
					row.StatusLabel = "No API data";
					vm.OtherCount++;
					vm.Rows.Add(row);
					continue;
				}

				var code = api.StatusShort ?? "";
				row.StatusShort = string.IsNullOrWhiteSpace(code) ? "-" : code;
				row.Elapsed = api.Elapsed;

				if (FootballFixtureStatuses.Finished.Contains(code))
				{
					row.State = AutoScoreState.Finished;
					row.StatusLabel = api.StatusLong ?? "Finished";
					row.HomeScore = api.HomeGoals;
					row.AwayScore = api.AwayGoals;
					row.HasPenalties = api.HasPenalties;
					if (api.HasPenalties && api.HomeIsWinner.HasValue)
						row.HomeWonOnPenalties = api.HomeIsWinner;
					vm.FinishedCount++;
				}
				else if (FootballFixtureStatuses.InProgress.Contains(code))
				{
					row.State = AutoScoreState.InProgress;
					row.StatusLabel = api.StatusLong ?? "In progress";
					row.HomeScore = api.HomeGoals;
					row.AwayScore = api.AwayGoals;
					vm.InProgressCount++;
				}
				else if (FootballFixtureStatuses.Abandoned.Contains(code))
				{
					row.State = AutoScoreState.Abandoned;
					row.StatusLabel = api.StatusLong ?? "Postponed / cancelled";
					vm.OtherCount++;
				}
				else
				{
					row.State = AutoScoreState.NotStarted;
					row.StatusLabel = api.StatusLong ?? "Not started";
					vm.OtherCount++;
				}

				vm.Rows.Add(row);
			}

			// Finished first, then in-progress, then the rest.
			vm.Rows = vm.Rows
				.OrderBy(r => r.State == AutoScoreState.Finished ? 0
							: r.State == AutoScoreState.InProgress ? 1 : 2)
				.ThenBy(r => r.KickoffUtc)
				.ToList();

			return vm;
		}

		public async Task<AutoScoreConfirmResultViewModel> ConfirmAutoScoresAsync(IEnumerable<Guid> matchIds)
		{
			var result = new AutoScoreConfirmResultViewModel();

			var ids = matchIds?.Distinct().ToList() ?? new List<Guid>();
			if (ids.Count == 0)
				return result;

			var events = await _dbContext.MatchEvents
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Where(m => ids.Contains(m.Id))
				.ToListAsync();

			// Re-fetch fresh results for just these fixtures; never trust a client-posted score.
			var fixtureIds = events
				.Where(m => m.ApiFixtureId != null)
				.Select(m => m.ApiFixtureId!.Value)
				.ToList();

			var apiResults = await _footballApi.GetFixtureResultsAsync(fixtureIds);
			var byId = apiResults.ToDictionary(r => r.ApiFixtureId);

			foreach (var m in events)
			{
				var label = $"{m.HomeTeam.Name} vs {m.AwayTeam.Name}";

				var row = new AutoScoreResultRow
				{
					HomeTeam = m.HomeTeam.Name,
					AwayTeam = m.AwayTeam.Name,
					HomeLogo = string.IsNullOrWhiteSpace(m.HomeTeam.LocalLogoUrl) ? m.HomeTeam.LogoUrl : m.HomeTeam.LocalLogoUrl,
					AwayLogo = string.IsNullOrWhiteSpace(m.AwayTeam.LocalLogoUrl) ? m.AwayTeam.LogoUrl : m.AwayTeam.LocalLogoUrl
				};

				if (m.LiveResult != null)
				{
					result.Skipped++;
					row.Outcome = "skipped";
					row.Detail = "Already scored";
					result.Messages.Add($"{label}: already scored - skipped.");
					result.Rows.Add(row);
					continue;
				}

				if (m.ApiFixtureId == null || !byId.TryGetValue(m.ApiFixtureId.Value, out var api))
				{
					result.Skipped++;
					row.Outcome = "skipped";
					row.Detail = "No API data";
					result.Messages.Add($"{label}: no API data - skipped.");
					result.Rows.Add(row);
					continue;
				}

				var code = api.StatusShort ?? "";
				if (!FootballFixtureStatuses.Finished.Contains(code) ||
					api.HomeGoals == null || api.AwayGoals == null)
				{
					result.Skipped++;
					row.Outcome = "skipped";
					row.Detail = $"Not finished ({api.StatusLong ?? code})";
					result.Messages.Add($"{label}: not finished yet ({api.StatusLong ?? code}) - skipped.");
					result.Rows.Add(row);
					continue;
				}

				try
				{
					// Step 1 - mark Full Time (same guarded path the manual flow uses).
					if (m.Status != MatchStatus.FullTime)
						await _liveMatchResultService.MarkMatchAsFullTimeAsync(m.Id);

					bool? homeWonPens = null;
					if (api.HasPenalties && api.HomeIsWinner.HasValue)
						homeWonPens = api.HomeIsWinner;

					// Step 2 - record result + settle bets (queues win/void emails via the existing chain).
					var ok = await _liveMatchResultService.AddMatchResultAsync(
						m.Id,
						api.HomeGoals.Value,
						api.AwayGoals.Value,
						homeWonPens,
						"Auto-scored from API");

					if (ok)
					{
						result.Scored++;
						row.Outcome = "scored";
						row.HomeScore = api.HomeGoals;
						row.AwayScore = api.AwayGoals;
						row.HasPenalties = api.HasPenalties;
						row.HomeWonOnPenalties = homeWonPens;
						var penNote = homeWonPens.HasValue
							? $" (pens: {(homeWonPens.Value ? m.HomeTeam.Name : m.AwayTeam.Name)})"
							: "";
						result.Messages.Add($"{label}: scored {api.HomeGoals}-{api.AwayGoals}{penNote}.");
					}
					else
					{
						result.Skipped++;
						row.Outcome = "skipped";
						row.Detail = "Could not be saved (may already have a result)";
						result.Messages.Add($"{label}: could not be saved (it may already have a result).");
					}
				}
				catch (Exception ex)
				{
					result.Skipped++;
					row.Outcome = "error";
					row.Detail = ex.Message;
					result.Messages.Add($"{label}: error - {ex.Message}");
				}

				result.Rows.Add(row);
			}

			return result;
		}
	}
}
