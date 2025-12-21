using MatchFixer.Infrastructure.Models.FootballAPI;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface IFootballApiService
	{
		Task FetchAndSaveFixturesAsync();
		Task FetchAndSaveTeamsAsync();
		Task<List<UpcomingMatchDto>> GetUpcomingFromApiAsync(int leagueId, int? limit = null); 
		int? ExtractTeamIdFromLogoUrl(string logoUrl);
	}
}
