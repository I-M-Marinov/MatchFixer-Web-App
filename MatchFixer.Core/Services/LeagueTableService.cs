using MatchFixer.Common.Enums;
using MatchFixer.Common.FootballCompetitions;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LeagueTables;
using MatchFixer.Infrastructure.Contracts;

namespace MatchFixer.Core.Services
{
	public class LeagueTableService : ILeagueTableService
	{
		private readonly ITheSportsDbApiService _api;
		private readonly ILiveMatchQueryService _liveQueryService;

		public LeagueTableService(
			ITheSportsDbApiService api,
			ILiveMatchQueryService liveQueryService)
		{
			_api = api;
			_liveQueryService = liveQueryService;
		}

		public async Task<List<LeagueTableRowViewModel>> GetLeagueTableAsync(
				InternalLeague league,
				string? season = null,
				CancellationToken ct = default)
		{
			// Resolve internal → external mapping
			if (!FootballLeagueMappings.Leagues.TryGetValue(league, out var mapping))
				return new();

			if (!mapping.TheSportsDbLeagueId.HasValue)
				return new();

			// Primary season format (TSDB prefers YYYY-YY)
			season ??= GetCurrentSeasonForTheSportsDb();

			// Primary API call
			var apiTable = await _api.GetLeagueTableAsync(
				mapping.TheSportsDbLeagueId.Value,
				season,
				ct);

			if (!apiTable.Any() && season.Contains('-'))
			{
				var fallbackSeason = season.Split('-')[0];

				apiTable = await _api.GetLeagueTableAsync(
					mapping.TheSportsDbLeagueId.Value,
					fallbackSeason,
					ct);
			}

			// No data available
			if (!apiTable.Any())
				return new();

			// Map API DTO → ViewModel
			var table = apiTable
				.OrderBy(r =>
					int.TryParse(r.Rank, out var rank)
						? rank
						: int.MaxValue)
				.Select(r => new LeagueTableRowViewModel
				{
					Rank = int.TryParse(r.Rank, out var rank) ? rank : 0,

					Team = r.Team ?? string.Empty,
					Badge = r.Badge ?? string.Empty,

					Played = int.TryParse(r.Played, out var played) ? played : 0,
					Wins = int.TryParse(r.Wins, out var wins) ? wins : 0,
					Draws = int.TryParse(r.Draws, out var draws) ? draws : 0,
					Losses = int.TryParse(r.Losses, out var losses) ? losses : 0,
					GoalDiff = int.TryParse(r.GoalDifference, out var gd) ? gd : 0,
					Points = int.TryParse(r.Points, out var pts) ? pts : 0,

					// defaults
					IsPlayingLive = false
				})
				.ToList();

			// LIVE MATCH MERGE 
			var liveMatches = await _liveQueryService
				.GetLiveMatchesForLeagueAsync(league, ct);

			if (liveMatches.Count > 0)
			{
				var liveLookup = liveMatches
					.ToDictionary(x => x.TeamName, StringComparer.OrdinalIgnoreCase);

				foreach (var row in table)
				{
					if (liveLookup.TryGetValue(row.Team, out var live))
					{
						row.IsPlayingLive = true;
						row.LiveOpponent = live.Opponent;
						row.LiveGoalsFor = live.GoalsFor;
						row.LiveGoalsAgainst = live.GoalsAgainst;
					}
				}
			}

			return table;
		}

		private static string GetCurrentSeasonForTheSportsDb()
		{
			var now = DateTime.UtcNow;

			var startYear = now.Month >= 7
				? now.Year
				: now.Year - 1;

			var endYear = startYear + 1;

			return $"{startYear}-{endYear}";
		}

	}
}
