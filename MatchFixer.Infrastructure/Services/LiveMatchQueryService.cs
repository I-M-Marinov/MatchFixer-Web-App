
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

			if (!mapping.TheSportsDbLeagueId.HasValue)
				return Array.Empty<LiveTeamMatchInfo>();

			var liveEvents = await _api.GetLiveEventsByLeagueAsync(
				mapping.TheSportsDbLeagueId.Value,
				ct);

			var result = new List<LiveTeamMatchInfo>();

			foreach (var e in liveEvents)
			{
				if (string.IsNullOrWhiteSpace(e.strHomeTeam) ||
				    string.IsNullOrWhiteSpace(e.strAwayTeam))
					continue;

				int.TryParse(e.intHomeScore, out var homeScore);
				int.TryParse(e.intAwayScore, out var awayScore);

				// Home team perspective
				result.Add(new LiveTeamMatchInfo
				{
					TeamName = e.strHomeTeam,
					Opponent = e.strAwayTeam,
					GoalsFor = homeScore,
					GoalsAgainst = awayScore
				});

				// Away team perspective
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
	}

}
