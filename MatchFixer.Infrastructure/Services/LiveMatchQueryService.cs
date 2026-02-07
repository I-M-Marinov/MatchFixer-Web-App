
using MatchFixer.Common.Enums;
using MatchFixer.Common.FootballCompetitions;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;

namespace MatchFixer.Infrastructure.Services
{
	public class LiveMatchQueryService : ILiveMatchQueryService
	{
		private readonly ITheSportsDbApiService _api;

		public LiveMatchQueryService(ITheSportsDbApiService api)
		{
			_api = api;
		}

		public async Task<IReadOnlyList<LiveTeamMatchInfo>>
			GetLiveMatchesForLeagueAsync(
				InternalLeague league,
				CancellationToken ct = default)
		{
			if (!FootballLeagueMappings.Leagues.TryGetValue(league, out var mapping))
				return Array.Empty<LiveTeamMatchInfo>();

			// Get all live soccer events (v2)
			var liveEvents = await _api.GetLiveSoccerEventsAsync(ct);

			var result = new List<LiveTeamMatchInfo>();

			foreach (var e in liveEvents)
			{
				// Exclude matches that already finished 
				if (IsFinished(e.strStatus))
					continue;

				// Validate teams
				if (string.IsNullOrWhiteSpace(e.strHomeTeam) ||
				    string.IsNullOrWhiteSpace(e.strAwayTeam))
					continue;

				int.TryParse(e.intHomeScore, out var homeScore);
				int.TryParse(e.intAwayScore, out var awayScore);

				// Home team view
				result.Add(new LiveTeamMatchInfo
				{
					TeamName = e.strHomeTeam,
					Opponent = e.strAwayTeam,
					GoalsFor = homeScore,
					GoalsAgainst = awayScore
				});

				// Away team view
				result.Add(new LiveTeamMatchInfo
				{
					TeamName = e.strAwayTeam,
					Opponent = e.strHomeTeam,
					GoalsFor = awayScore,
					GoalsAgainst = homeScore
				});
			}

			return result;
		}

		private static bool IsFinished(string? status)
		{
			if (string.IsNullOrWhiteSpace(status))
				return false;

			return status.Equals("FT", StringComparison.OrdinalIgnoreCase)
			       || status.Equals("AET", StringComparison.OrdinalIgnoreCase)
			       || status.Equals("PEN", StringComparison.OrdinalIgnoreCase);
		}

	}

}
