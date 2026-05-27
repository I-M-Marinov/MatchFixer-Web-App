using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.WordCup;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class WorldCupService : IWorldCupService
	{
		private readonly MatchFixerDbContext _context;
		private readonly ITheSportsDbApiService _sportsDbApiService;

		public WorldCupService(MatchFixerDbContext context, ITheSportsDbApiService sportsDbApiService)
		{
			_context = context;
			_sportsDbApiService = sportsDbApiService;
		}

		public async Task<WorldCupPageViewModel> GetWorldCupPageAsync()
		{
			var matches = await _context.WorldCupMatches

				.OrderBy(x => x.Stage)
				.ThenBy(x => x.RoundPosition)

				.ToListAsync();

			var groupedStages = matches
				.GroupBy(x => x.Stage)
				.Select(g => new WorldCupStageViewModel
				{
					StageName = FormatStageName(g.Key),

					Matches = g
						.OrderBy(x => x.RoundPosition)
						.Select(MapMatchCard)
						.ToList()
				})
				.ToList();

			return new WorldCupPageViewModel
			{
				Stages = groupedStages,

				Bracket = BuildBracket(matches),

				GroupStandings =
					await GetGroupStandingsAsync()
			};
		}

		private KnockoutBracketViewModel BuildBracket(
			List<WorldCupMatch> matches)
		{
			return new KnockoutBracketViewModel
			{
				RoundOf16 = MapStage(matches, WorldCupStage.RoundOf16),

				QuarterFinals = MapStage(matches, WorldCupStage.QuarterFinal),

				SemiFinals = MapStage(matches, WorldCupStage.SemiFinal),

				ThirdPlace = MapStage(matches, WorldCupStage.ThirdPlace),

				Final = MapStage(matches, WorldCupStage.Final)
			};
		}

		private List<WorldCupMatchCardViewModel> MapStage(
			List<WorldCupMatch> matches,
			WorldCupStage stage)
		{
			return matches
				.Where(x => x.Stage == stage)
				.OrderBy(x => x.RoundPosition)
				.Select(MapMatchCard)
				.ToList();
		}

		private WorldCupMatchCardViewModel MapMatchCard(
			WorldCupMatch match)
		{
			return new WorldCupMatchCardViewModel
			{
				MatchId = match.Id,

				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,

				HomeLogo = match.HomeLogo,
				AwayLogo = match.AwayLogo,

				MatchDate = match.MatchDate,

				HomeScore = match.HomeScore,
				AwayScore = match.AwayScore,

				IsFinished = match.IsFinished,

				IsLive = match.IsLive,

				Stage = match.Stage
			};
		}

		private string FormatStageName(WorldCupStage stage)
		{
			return stage switch
			{
				WorldCupStage.GroupStage => "Group Stage",
				WorldCupStage.RoundOf16 => "Round of 16",
				WorldCupStage.QuarterFinal => "Quarter Finals",
				WorldCupStage.SemiFinal => "Semi Finals",
				WorldCupStage.ThirdPlace => "Third Place",
				WorldCupStage.Final => "Final",
				_ => stage.ToString()
			};
		}

		public async Task<List<WorldCupGroupStandingViewModel>>
			GetGroupStandingsAsync()
		{
			var standings = await _sportsDbApiService
				.GetLeagueTableAsync(
					4424,
					"2026");

			if (!standings.Any())
			{
				return new();
			}

			// TEMP:
			// Since API DTO has no group field,
			// we fake groups based on ranking chunks

			var grouped = standings
				.Select((team, index) => new
				{
					Team = team,

					GroupName =
						$"Group {(char)('A' + (index / 4))}"
				})
				.GroupBy(x => x.GroupName)
				.OrderBy(x => x.Key)
				.Select(group => new WorldCupGroupStandingViewModel
				{
					GroupName = group.Key,

					Teams = group
						.OrderByDescending(x =>
							ParseInt(x.Team.Points))

						.ThenByDescending(x =>
							ParseInt(x.Team.GoalDifference))

						.Select(x => new WorldCupStandingTeamViewModel
						{
							TeamName = x.Team.Team,

							TeamLogo = string.IsNullOrWhiteSpace(
								x.Team.Badge)
								? "/images/default-team.png"
								: x.Team.Badge,

							Played =
								ParseInt(x.Team.Played),

							Wins =
								ParseInt(x.Team.Wins),

							Draws =
								ParseInt(x.Team.Draws),

							Losses =
								ParseInt(x.Team.Losses),

							GoalDifference =
								ParseInt(x.Team.GoalDifference),

							Points =
								ParseInt(x.Team.Points),

							IsQualified = group
								.OrderByDescending(t =>
									ParseInt(t.Team.Points))

								.Take(2)

								.Any(t =>
									t.Team.Team == x.Team.Team)
						})
						.ToList()
				})
				.ToList();

			return grouped;
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