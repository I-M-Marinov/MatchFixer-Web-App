using MatchFixer.Infrastructure.Models.FootballAPI;


namespace MatchFixer.Core.Contracts
{
	public interface IUpcomingMatchService
	{
		Task<List<UpcomingMatchDto>> GetUpcomingMatchesAsync(int leagueId, int take = 20);
	}
}
