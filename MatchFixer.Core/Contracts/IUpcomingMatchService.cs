using MatchFixer.Infrastructure.Models.FootballAPI;


namespace MatchFixer.Core.Contracts
{
	public interface IUpcomingMatchService
	{
		Task<List<UpcomingMatchRowViewModel>> GetUpcomingMatchesAsync(int leagueId, int take = 20);
	}
}
