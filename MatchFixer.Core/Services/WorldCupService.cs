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

			// Restrict to FIFA World Cup events only 
			var matchEvents = await _context.MatchEvents
				.Include(x => x.HomeTeam)
				.Include(x => x.AwayTeam)
				.Include(x => x.LiveResult)
				.Where(x => !x.IsCancelled && x.CompetitionName == "FIFA World Cup")
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

			var matchDateUtc  = match.MatchDate;
			var dateHasPassed = matchDateUtc <= DateTime.UtcNow;

			var isFinished = liveResult != null
				|| (match.IsFinished && dateHasPassed);

			var isOngoing = !isFinished && !match.IsLive && dateHasPassed;

			var homeScore = isFinished ? (liveResult?.HomeScore ?? match.HomeScore) : (int?)null;
			var awayScore = isFinished ? (liveResult?.AwayScore ?? match.AwayScore) : (int?)null;

			return new WorldCupMatchCardViewModel
			{
				MatchId = match.Id,

				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,

				HomeLogo = match.HomeLogo,
				AwayLogo = match.AwayLogo,

				MatchDate = matchDateLocal,

				HomeScore = homeScore,
				AwayScore = awayScore,

				IsFinished = isFinished,
				IsLive = match.IsLive,
				IsOngoing = isOngoing,

				Winner = isFinished
					? homeScore > awayScore ? match.HomeTeam
					: awayScore > homeScore ? match.AwayTeam
					: null
					: null,

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

			// Build stage date map once — used to infer stage for fixtures missing intRound.
			var stageDateMap = BuildStageDateMap(knockoutFixtures);

			var updatedCount = 0;

			foreach (var fixture in knockoutFixtures)
			{
				if (!int.TryParse(fixture.EventId, out var apiId))
					continue;

				var match = existingMatches.FirstOrDefault(m => m.ApiEventId == apiId);
				if (match == null)
					continue;

				match.HomeTeam = fixture.HomeTeam;
				match.AwayTeam = fixture.AwayTeam;
				match.HomeLogo = fixture.HomeBadge ?? match.HomeLogo;
				match.AwayLogo = fixture.AwayBadge ?? match.AwayLogo;

				match.HomeScore  = int.TryParse(fixture.HomeScore, out var hs)  ? hs  : null;
				match.AwayScore  = int.TryParse(fixture.AwayScore, out var aws) ? aws : null;
				match.IsFinished = fixture.Status == StatusMatchFinished;
				match.IsLive     = fixture.Status == StatusLive;

				// Re-classify stage when API now has intRound, or infer from date.
				var updatedStage = ParseWorldCupStageFromRound(fixture.Round, fixture.Group);
				if (updatedStage is null && DateTime.TryParse($"{fixture.Date} {fixture.Time}", out var kickoff))
					updatedStage = InferStageFromDate(kickoff, stageDateMap);
				if (updatedStage.HasValue && updatedStage.Value != WorldCupStage.GroupStage)
					match.Stage = updatedStage.Value;

				updatedCount++;
			}

			await _context.SaveChangesAsync();

			var allKnockout = await _context.WorldCupMatches
				.Where(m => m.IsKnockout)
				.OrderBy(m => m.MatchDate)
				.ToListAsync();

			CapKnockoutStages(allKnockout);
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

			// Fixtures with no strGroup are candidates for knockout rounds.
			// However the API also returns Group Stage matchday 3 (June 24-28) with
			// an empty strGroup, causing them to be misclassified as Third Place.
			// Disambiguate by counting fixtures per round: knockout stages have few
			// fixtures per round (Final=1, Third=1, SF=2, QF=4), while group-stage
			// matchdays produce many (12+ per matchday). Guard only applies to the
			// ambiguous low-value rounds 1, 2, 3 — rounds ≥ 4 are unambiguous.
			var noGroupFixtures = fixtures
				.Where(f => string.IsNullOrWhiteSpace(f.Group))
				.ToList();

			var countByRound = noGroupFixtures
				.GroupBy(f => f.Round)
				.ToDictionary(g => g.Key, g => g.Count());

			var knockoutFixtures = noGroupFixtures.Where(f =>
			{
				if (!int.TryParse(f.Round, out var r)) return true;
				if (r is 32 or 16 or 8 or 4) return true; // unambiguous knockout rounds
				// For ambiguous rounds (1, 2, 3): only treat as knockout if ≤ 4 fixtures
				// share that round value (group-stage matchdays have 12+ per round).
				return countByRound.GetValueOrDefault(f.Round, 0) <= 4;
			}).ToList();

			if (!knockoutFixtures.Any())
			{
				return 0;
			}

			// Determine the next RoundPosition to avoid collisions with existing group-stage rows.
			var maxPosition = await _context.WorldCupMatches
				.MaxAsync(m => (int?)m.RoundPosition) ?? 0;

			// Build a stage→date map from fixtures whose intRound IS known so we can
			// infer the stage of fixtures where the API hasn't assigned intRound yet.
			var stageDateMap = BuildStageDateMap(knockoutFixtures);

			var newMatches = new List<WorldCupMatch>();

			foreach (var fixture in knockoutFixtures)
			{
				var stage = ParseWorldCupStageFromRound(fixture.Round, fixture.Group);

				// If intRound is missing, fall back to date-based inference.
				// e.g. a fixture dated after the last R16 match → Quarter-Final.
				if (stage is null && DateTime.TryParse($"{fixture.Date} {fixture.Time}", out var kickoffForInference))
					stage = InferStageFromDate(kickoffForInference, stageDateMap);

				// Still unknown — skip until the API provides usable data.
				if (stage is null)
					continue;

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

					Stage = stage.Value,
					GroupName = null,
					IsKnockout = true,
					IsFinished = fixture.Status == StatusMatchFinished,
					IsLive = fixture.Status == StatusLive,
					RoundPosition = ++maxPosition
				});
			}

			// Correct any stages that were over-assigned due to API returning the same
			// intRound value for multiple stages (e.g. intRound=8 for both QF and SF).
			CapKnockoutStages(newMatches);

			await _context.WorldCupMatches.AddRangeAsync(newMatches);
			await _context.SaveChangesAsync();

			return newMatches.Count;
		}

		/// <summary>
		/// Maps TheSportsDB <c>intRound</c> + <c>strGroup</c> to a <see cref="WorldCupStage"/>.
		/// Returns <c>null</c> when the stage cannot be determined from the round value alone.
		/// Callers should attempt date-based inference before giving up.
		/// </summary>
		private static WorldCupStage? ParseWorldCupStageFromRound(
			string? round,
			string? group = null)
		{
			// TheSportsDB uses intRound as the literal remaining-team count for 2026:
			//   1, 2, 3 with strGroup  → Group Stage matchdays
			//   32                     → Round of 32
			//   16                     → Round of 16
			//   8                      → Quarter-Final
			//   4                      → Semi-Final
			//   3 with no strGroup     → Third Place play-off
			//   1 or 2 with no strGroup→ Final
			if (int.TryParse(round, out var intRound))
			{
				// intRound 1–3 are ambiguous: group matchday OR late-stage knockout.
				// Disambiguate via strGroup presence.
				if (intRound <= 3 && !string.IsNullOrWhiteSpace(group))
					return WorldCupStage.GroupStage;

				return intRound switch
				{
					1 or 2 => WorldCupStage.Final,
					3      => WorldCupStage.ThirdPlace,
					4      => WorldCupStage.SemiFinal,
					8      => WorldCupStage.QuarterFinal,
					16     => WorldCupStage.RoundOf16,
					32     => WorldCupStage.RoundOf32,
					_      => null   // unrecognised number — try date inference
				};
			}

			// Empty/null round with a known group → Group Stage.
			if (!string.IsNullOrWhiteSpace(group))
				return WorldCupStage.GroupStage;

			// Fallback: text-based matching for non-numeric round strings.
			if (!string.IsNullOrWhiteSpace(round))
			{
				var r = round.ToLower();
				if (r.Contains("round of 32")) return WorldCupStage.RoundOf32;
				if (r.Contains("round of 16")) return WorldCupStage.RoundOf16;
				if (r.Contains("quarter"))     return WorldCupStage.QuarterFinal;
				if (r.Contains("semi"))        return WorldCupStage.SemiFinal;
				if (r.Contains("third"))       return WorldCupStage.ThirdPlace;
				if (r.Contains("final"))       return WorldCupStage.Final;
			}

			// round is empty/null AND group is empty/null: definitely a knockout fixture
			// but the API hasn't provided intRound yet — caller must use date inference.
			return null;
		}

		/// <summary>
		/// Infers the knockout stage for a fixture whose <c>intRound</c> is missing,
		/// by comparing its date to the date ranges of other fixtures that DO have a
		/// known stage.  Knockout stages are chronologically ordered, so if a fixture
		/// falls after the last Round-of-16 date it must be a Quarter-Final, etc.
		/// </summary>
		private static WorldCupStage? InferStageFromDate(
			DateTime matchDate,
			IReadOnlyDictionary<WorldCupStage, (DateTime Min, DateTime Max)> stageDateMap)
		{
			// Chronological knockout order
			var order = new[]
			{
				WorldCupStage.RoundOf32,
				WorldCupStage.RoundOf16,
				WorldCupStage.QuarterFinal,
				WorldCupStage.SemiFinal,
				WorldCupStage.ThirdPlace,
				WorldCupStage.Final,
			};

			// 1. Exact date-window match (date falls within a known stage's range)
			foreach (var stage in order)
			{
				if (!stageDateMap.TryGetValue(stage, out var range)) continue;
				if (matchDate.Date >= range.Min.Date && matchDate.Date <= range.Max.Date)
					return stage;
			}

			// 2. Gap inference: if the date falls in the gap between two consecutive
			//    known stages, it belongs to the later of the two.
			for (int i = 0; i < order.Length - 1; i++)
			{
				var curr = order[i];
				var next = order[i + 1];
				if (!stageDateMap.TryGetValue(curr, out var currRange)) continue;
				if (!stageDateMap.TryGetValue(next, out var nextRange)) continue;

				if (matchDate.Date > currRange.Max.Date && matchDate.Date < nextRange.Min.Date)
					return next;
			}

			// 3. After the last known stage — it belongs to the following stage in order.
			for (int i = order.Length - 1; i >= 0; i--)
			{
				if (!stageDateMap.TryGetValue(order[i], out var range)) continue;
				if (matchDate.Date > range.Max.Date && i + 1 < order.Length)
					return order[i + 1];
				break;
			}

			return null;
		}

		/// <summary>
		/// Maximum number of fixtures allowed per knockout stage in the 48-team 2026 World Cup.
		/// TheSportsDB sometimes assigns the same <c>intRound</c> value (e.g. 8) to both
		/// Quarter-Finals and Semi-Finals, causing later matches to be misclassified.
		/// After the main classification loop, <see cref="CapKnockoutStages"/> redistributes
		/// any excess fixtures to the chronologically next stage.
		/// </summary>
		private static readonly Dictionary<WorldCupStage, (int Max, WorldCupStage Next)> StageCaps = new()
		{
			{ WorldCupStage.RoundOf32,    (16, WorldCupStage.RoundOf16)    },
			{ WorldCupStage.RoundOf16,    ( 8, WorldCupStage.QuarterFinal) },
			{ WorldCupStage.QuarterFinal, ( 4, WorldCupStage.SemiFinal)    },
			{ WorldCupStage.SemiFinal,    ( 2, WorldCupStage.ThirdPlace)   },
			{ WorldCupStage.ThirdPlace,   ( 1, WorldCupStage.Final)        },
		};

		/// <summary>
		/// Corrects over-assigned knockout stages by capping each stage at its known
		/// maximum and re-labelling chronologically later fixtures to the next stage.
		/// Processed in forward order so cascading corrections propagate correctly
		/// (e.g. QF overflow → SF, SF overflow → ThirdPlace).
		/// </summary>
		private static void CapKnockoutStages(List<WorldCupMatch> matches)
		{
			var order = new[]
			{
				WorldCupStage.RoundOf32,
				WorldCupStage.RoundOf16,
				WorldCupStage.QuarterFinal,
				WorldCupStage.SemiFinal,
				WorldCupStage.ThirdPlace,
			};

			foreach (var stage in order)
			{
				if (!StageCaps.TryGetValue(stage, out var cap)) continue;

				var stageMatches = matches
					.Where(m => m.Stage == stage)
					.OrderBy(m => m.MatchDate)
					.ToList();

				if (stageMatches.Count <= cap.Max) continue;

				foreach (var m in stageMatches.Skip(cap.Max))
					m.Stage = cap.Next;
			}
		}

		/// <summary>
		/// Builds a stage → (earliest date, latest date) map from the subset of
		/// knockout fixtures whose <c>intRound</c> is already known.
		/// </summary>
		private static Dictionary<WorldCupStage, (DateTime Min, DateTime Max)>
			BuildStageDateMap(IEnumerable<WorldCupFixtureApiDto> knockoutFixtures)
		{
			var map = new Dictionary<WorldCupStage, (DateTime Min, DateTime Max)>();

			foreach (var f in knockoutFixtures)
			{
				var stage = ParseWorldCupStageFromRound(f.Round, f.Group);
				if (stage is null || stage == WorldCupStage.GroupStage) continue;
				if (!DateTime.TryParse($"{f.Date} {f.Time}", out var d)) continue;

				if (map.TryGetValue(stage.Value, out var existing))
					map[stage.Value] = (d < existing.Min ? d : existing.Min,
					                    d > existing.Max ? d : existing.Max);
				else
					map[stage.Value] = (d, d);
			}

			return map;
		}

		public async Task<List<BracketMatchOrderDto>> GetKnockoutMatchOrderAsync()
		{
			return await _context.WorldCupMatches
				.AsNoTracking()
				.Where(m => m.IsKnockout)
				.OrderBy(m => m.Stage)
				.ThenBy(m => m.RoundPosition)
				.Select(m => new BracketMatchOrderDto(
					m.Id,
					m.HomeTeam,
					m.AwayTeam,
					m.HomeLogo,
					m.AwayLogo,
					m.HomeScore,
					m.AwayScore,
					m.Stage,
					m.RoundPosition))
				.ToListAsync();
		}

		public async Task SaveBracketOrderAsync(IEnumerable<(int Id, int Position)> order)
		{
			var ids = order.Select(x => x.Id).ToList();

			var matches = await _context.WorldCupMatches
				.Where(m => ids.Contains(m.Id))
				.ToListAsync();

			var positionMap = order.ToDictionary(x => x.Id, x => x.Position);

			foreach (var match in matches)
			{
				if (positionMap.TryGetValue(match.Id, out var newPos))
					match.RoundPosition = newPos;
			}

			await _context.SaveChangesAsync();
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