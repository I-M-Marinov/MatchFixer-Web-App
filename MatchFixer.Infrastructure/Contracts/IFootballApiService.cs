using MatchFixer.Infrastructure.Models.FootballAPI;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface IFootballApiService
	{
		Task FetchAndSaveFixturesAsync();
		Task FetchAndSaveTeamsAsync();
		Task<List<UpcomingMatchDto>> GetUpcomingFromApiAsync(int leagueId, int? limit = null); 
		int? ExtractTeamIdFromLogoUrl(string logoUrl);
		Task<IReadOnlyList<FixtureResultDto>> GetFixtureResultsAsync(IReadOnlyCollection<int> fixtureIds, CancellationToken ct = default);
	}
}
