using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.WordCup;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Common.GeneralConstants.WorldCupApiConstants;

namespace MatchFixer.Core.Services
{
	public class WorldCupService : IWorldCupService
	{
		private readonly MatchFixerDbContext _context;
		private readonly ITheSportsDbApiService _sportsDbApiService;
		private readonly ITimezoneService _timezoneService;
		private readonly IUserContextService _userContextService;

		public WorldCupService(MatchFixerDbContext context, ITheSportsDbApiService sportsDbApiService, ITimezoneService timezoneService, IUserContextService userContextService)
		{
			_context = context;
			_sportsDbApiService = sportsDbApiService;
			_timezoneService = timezoneService;
			_userContextService = userContextService;
		}

		public async Task<WorldCupPageViewModel> GetWorldCupPageAsync()
		{
			var matches = await _context.WorldCupMatches

				.OrderBy(x => x.Stage)
				.ThenBy(x => x.RoundPosition)
				.ToListAsync();

			var user = await _userContextService.GetCurrentUserAsync();
			var userTimeZone = user.TimeZone;

			var matchEvents = await _context.MatchEvents
				.Include(x => x.HomeTeam)
				.Include(x => x.AwayTeam)
				.Include(x => x.LiveResult)
				.Where(x => !x.IsCancelled)
				.ToListAsync();

			var groupedStages = matches
				.GroupBy(x => x.Stage)
				.Select(g => new WorldCupStageViewModel
				{
					StageName = FormatStageName(g.Key),

					DayGroups = g
						.OrderBy(x => x.MatchDate)
						.GroupBy(x =>
							DateOnly.FromDateTime(
								_timezoneService
									.ConvertToUserTime(
										x.MatchDate,
										userTimeZone)!
									.Value))
						.Select(day => new WorldCupDayGroupViewModel
						{
							Date = day.Key,

							Matches = day
								.Select(x => MapMatchCard(
									x,
									matchEvents,
									userTimeZone))
								.ToList()
						})
						.ToList()
				})
				.ToList();

			return new WorldCupPageViewModel
			{
				Stages = groupedStages,

				Bracket = BuildBracket(
					matches,
					matchEvents,
					userTimeZone),

				GroupStandings =
					await GetGroupStandingsAsync()
			};
		}

		private KnockoutBracketViewModel BuildBracket(
			List<WorldCupMatch> matches,
			List<MatchEvent> matchEvents,
			string userTimeZone)
		{
			return new KnockoutBracketViewModel
			{
				RoundOf32 = MapStage(
					matches,
					WorldCupStage.RoundOf32,
					matchEvents,
					userTimeZone),

				RoundOf16 = MapStage(
					matches,
					WorldCupStage.RoundOf16,
					matchEvents,
					userTimeZone),

				QuarterFinals = MapStage(
					matches,
					WorldCupStage.QuarterFinal,
					matchEvents,
					userTimeZone),

				SemiFinals = MapStage(
					matches,
					WorldCupStage.SemiFinal,
					matchEvents,
					userTimeZone),

				ThirdPlace = MapStage(
					matches,
					WorldCupStage.ThirdPlace,
					matchEvents,
					userTimeZone),

				Final = MapStage(
					matches,
					WorldCupStage.Final,
					matchEvents,
					userTimeZone)
			};
		}

		private List<WorldCupMatchCardViewModel> MapStage(
			List<WorldCupMatch> matches,
			WorldCupStage stage,
			List<MatchEvent> matchEvents,
			string userTimeZone)
		{
			return matches
				.Where(x => x.Stage == stage)
				.OrderBy(x => x.RoundPosition)
				.Select(x => MapMatchCard(
					x,
					matchEvents,
					userTimeZone))
				.ToList();
		}

		private WorldCupMatchCardViewModel MapMatchCard(
			WorldCupMatch match,
			List<MatchEvent> matchEvents,
			string userTimeZone)
		{
			var matchDateLocal =
				_timezoneService
					.ConvertToUserTime(
						match.MatchDate,
						userTimeZone)
					;

			var matchEvent = matchEvents.FirstOrDefault(x =>
				x.HomeTeam.Name == match.HomeTeam &&
				x.AwayTeam.Name == match.AwayTeam);

			var liveResult = matchEvent?.LiveResult;
			var isFinished = match.IsFinished || liveResult != null;
			var isOngoing = !isFinished && !match.IsLive && match.MatchDate <= DateTime.UtcNow;

			return new WorldCupMatchCardViewModel
			{
				MatchId = match.Id,

				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,

				HomeLogo = match.HomeLogo,
				AwayLogo = match.AwayLogo,

				MatchDate = matchDateLocal,

				HomeScore = liveResult?.HomeScore ?? match.HomeScore,
				AwayScore = liveResult?.AwayScore ?? match.AwayScore,

				IsFinished = isFinished,
				IsLive = match.IsLive,
				IsOngoing = isOngoing,

				Stage = match.Stage,

				IsAvailableForBetting = matchEvent != null
					&& !isFinished
					&& !match.IsLive
					&& !isOngoing,

				MatchEventId = matchEvent?.Id
			};
		}

		private string FormatStageName(WorldCupStage stage)
		{
			return stage switch
			{
				WorldCupStage.GroupStage => "Group Stage",
				WorldCupStage.RoundOf32 => StageNameRoundOf32,
				WorldCupStage.RoundOf16 => StageNameRoundOf16,
				WorldCupStage.QuarterFinal => StageNameQuarterFinal,
				WorldCupStage.SemiFinal => StageNameSemiFinal,
				WorldCupStage.ThirdPlace => StageNameThirdPlace,
				WorldCupStage.Final => StageNameFinal,
				_ => stage.ToString()
			};
		}

		public async Task<List<WorldCupGroupStandingViewModel>>
			GetGroupStandingsAsync()
		{
			var rows = await _context.WorldCupGroupStandings
				.OrderBy(s => s.GroupName)
				.ThenBy(s => s.Rank)
				.ToListAsync();

			if (!rows.Any())
			{
				return new();
			}

			return rows
				.GroupBy(s => s.GroupName)
				.Select(group => new WorldCupGroupStandingViewModel
				{
					GroupName = group.Key,

					Teams = group
						.Select(s => new WorldCupStandingTeamViewModel
						{
							TeamName       = s.TeamName,
							TeamLogo       = s.TeamBadge,
							Played         = s.Played,
							Wins           = s.Wins,
							Draws          = s.Draws,
							Losses         = s.Losses,
							GoalDifference = s.GoalDifference,
							Points         = s.Points,
							IsQualified    = s.IsQualified
						})
						.ToList()
				})
				.ToList();
		}

		public async Task<int> RefreshGroupStandingsAsync()
		{
			var apiRows = await _sportsDbApiService
				.GetLeagueTableAsync(4429, "2026");

			if (!apiRows.Any())
			{
				return 0;
			}

			var newStandings = apiRows
				.Where(t => !string.IsNullOrWhiteSpace(t.Group))
				.Select(t => new WorldCupGroupStanding
				{
					GroupName      = t.Group,
					Rank           = ParseInt(t.Rank),
					TeamName       = t.Team,
					TeamBadge      = string.IsNullOrWhiteSpace(t.Badge)
						? "/images/default-team.png"
						: t.Badge,
					Played         = ParseInt(t.Played),
					Wins           = ParseInt(t.Wins),
					Draws          = ParseInt(t.Draws),
					Losses         = ParseInt(t.Losses),
					GoalDifference = ParseInt(t.GoalDifference),
					Points         = ParseInt(t.Points),
					IsQualified    = t.Description
						.Equals(DescriptionRoundOf32,
							StringComparison.OrdinalIgnoreCase),
					LastUpdated    = DateTime.UtcNow
				})
				.ToList();

			// Wipe existing standings and replace with fresh data.
			var existing = await _context.WorldCupGroupStandings.ToListAsync();
			_context.WorldCupGroupStandings.RemoveRange(existing);
			await _context.WorldCupGroupStandings.AddRangeAsync(newStandings);
			await _context.SaveChangesAsync();

			return newStandings.Count;
		}

		public async Task<int> RefreshKnockoutStageAsync()
		{
			var fixtures = await _sportsDbApiService
				.GetWorldCupFixturesAsync();

			// The API uses intRound as the literal round size (32, 16, 8, 4, …).
			// Group-stage matchdays are 1, 2, 3 and have a non-empty strGroup.
			// Knockout fixtures have strGroup = "" — that's the most reliable filter.
			var knockoutFixtures = fixtures
				.Where(f => string.IsNullOrWhiteSpace(f.Group))
				.ToList();

			if (!knockoutFixtures.Any())
			{
				return 0;
			}

			var apiEventIds = knockoutFixtures
				.Select(f =>
					int.TryParse(f.EventId, out var id) ? (int?)id : null)
				.Where(id => id.HasValue)
				.Select(id => id!.Value)
				.ToList();

			var existingMatches = await _context.WorldCupMatches
				.Where(m => m.IsKnockout
					&& m.ApiEventId.HasValue
					&& apiEventIds.Contains(m.ApiEventId.Value))
				.ToListAsync();

			var updatedCount = 0;

			foreach (var fixture in knockoutFixtures)
			{
				if (!int.TryParse(fixture.EventId, out var apiId))
				{
					continue;
				}

				var match = existingMatches
					.FirstOrDefault(m => m.ApiEventId == apiId);

				if (match == null)
				{
					continue;
				}

				match.HomeTeam = fixture.HomeTeam;
				match.AwayTeam = fixture.AwayTeam;
				match.HomeLogo = fixture.HomeBadge ?? match.HomeLogo;
				match.AwayLogo = fixture.AwayBadge ?? match.AwayLogo;

				match.HomeScore =
					int.TryParse(fixture.HomeScore, out var hs)
						? hs
						: null;

				match.AwayScore =
					int.TryParse(fixture.AwayScore, out var aws)
						? aws
						: null;

				match.IsFinished =
					fixture.Status == StatusMatchFinished;

				match.IsLive =
					fixture.Status == StatusLive;

				updatedCount++;
			}

			await _context.SaveChangesAsync();

			return updatedCount;
		}

		public async Task<int> ReclassifyAndRefreshAsync()
		{
			var toRemove = await _context.WorldCupMatches
				.Where(m => m.IsKnockout
					|| (m.Stage == WorldCupStage.GroupStage
						&& m.GroupName != null
						&& m.GroupName.ToLower().Contains("round of 32")))
				.ToListAsync();

			_context.WorldCupMatches.RemoveRange(toRemove);
			await _context.SaveChangesAsync();

			// Re-fetch all fixtures and re-seed knockout + R32 rows.
			var fixtures = await _sportsDbApiService
				.GetWorldCupFixturesAsync();

			var knockoutFixtures = fixtures
				.Where(f => string.IsNullOrWhiteSpace(f.Group))
				.ToList();

			if (!knockoutFixtures.Any())
			{
				return 0;
			}

			// Determine the next RoundPosition to avoid collisions with existing group-stage rows.
			var maxPosition = await _context.WorldCupMatches
				.MaxAsync(m => (int?)m.RoundPosition) ?? 0;

			var newMatches = new List<WorldCupMatch>();

			foreach (var fixture in knockoutFixtures)
			{
				var stage = ParseWorldCupStageFromRound(
					fixture.Round,
					fixture.Group);

				newMatches.Add(new WorldCupMatch
				{
					ApiEventId =
						int.TryParse(fixture.EventId, out var pid)
							? pid
							: null,

					HomeTeam = fixture.HomeTeam,
					AwayTeam = fixture.AwayTeam,
					HomeLogo = fixture.HomeBadge ?? string.Empty,
					AwayLogo = fixture.AwayBadge ?? string.Empty,

					MatchDate =
						DateTime.TryParse(
							$"{fixture.Date} {fixture.Time}",
							out var kickoff)
							? kickoff
							: DateTime.UtcNow,

					HomeScore =
						int.TryParse(fixture.HomeScore, out var hs)
							? hs
							: null,

					AwayScore =
						int.TryParse(fixture.AwayScore, out var aws)
							? aws
							: null,

					Stage = stage,
					GroupName = null,
					IsKnockout = true,
					IsFinished = fixture.Status == StatusMatchFinished,
					IsLive = fixture.Status == StatusLive,
					RoundPosition = ++maxPosition
				});
			}

			await _context.WorldCupMatches.AddRangeAsync(newMatches);
			await _context.SaveChangesAsync();

			return newMatches.Count;
		}

		private static WorldCupStage ParseWorldCupStageFromRound(
			string? round,
			string? group = null)
		{
			// TheSportsDB uses intRound as the literal remaining-team count:
			//   1, 2, 3  → Group Stage matchdays (strGroup is non-empty, e.g. "A")
			//   32       → Round of 32
			//   16       → Round of 16
			//   8        → Quarter-Final
			//   4        → Semi-Final
			//   3        → Third Place (intRound=3 but strGroup is empty)
			//   2 or 1   → Final
			if (int.TryParse(round, out var intRound))
			{
				// intRound 3 is ambiguous: group-stage matchday 3 OR third-place match.
				// Disambiguate via strGroup — knockout fixtures have an empty group.
				if (intRound == 3 && !string.IsNullOrWhiteSpace(group))
					return WorldCupStage.GroupStage;

				return intRound switch
				{
					1 or 2 => WorldCupStage.GroupStage,
					3      => WorldCupStage.ThirdPlace, // group is empty here
					4      => WorldCupStage.SemiFinal,
					8      => WorldCupStage.QuarterFinal,
					16     => WorldCupStage.RoundOf16,
					32     => WorldCupStage.RoundOf32,
					_      => WorldCupStage.Final       // 0, 64, or any other value
				};
			}

			if (string.IsNullOrWhiteSpace(round))
				return WorldCupStage.GroupStage;

			// Fallback: text-based matching for any future API changes.
			var r = round.ToLower();
			if (r.Contains("round of 32")) return WorldCupStage.RoundOf32;
			if (r.Contains("round of 16")) return WorldCupStage.RoundOf16;
			if (r.Contains("quarter"))     return WorldCupStage.QuarterFinal;
			if (r.Contains("semi"))        return WorldCupStage.SemiFinal;
			if (r.Contains("third"))       return WorldCupStage.ThirdPlace;
			if (r.Contains("final"))       return WorldCupStage.Final;

			return WorldCupStage.GroupStage;
		}

		private int ParseInt(string? value)
		{
			if (int.TryParse(value, out var parsed))
			{
				return parsed;
			}

			return 0;
		}
	}
}