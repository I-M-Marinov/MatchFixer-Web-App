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

			var leagueId = mapping.TheSportsDbLeagueId.Value.ToString();

			var liveEvents = await _api.GetLiveSoccerEventsAsync(ct);

			liveEvents = liveEvents
				.Where(e => !string.IsNullOrWhiteSpace(e.idLeague) &&
				            e.idLeague == leagueId)
				.ToList();

			var result = new List<LiveTeamMatchInfo>();

			foreach (var e in liveEvents)
			{
				if (IsFinished(e.strStatus))
					continue;

				if (string.IsNullOrWhiteSpace(e.strHomeTeam) ||
				    string.IsNullOrWhiteSpace(e.strAwayTeam))
					continue;

				int.TryParse(e.intHomeScore, out var homeScore);
				int.TryParse(e.intAwayScore, out var awayScore);

				result.Add(new LiveTeamMatchInfo
				{
					TeamName = e.strHomeTeam,
					Opponent = e.strAwayTeam,
					GoalsFor = homeScore,
					GoalsAgainst = awayScore
				});

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
