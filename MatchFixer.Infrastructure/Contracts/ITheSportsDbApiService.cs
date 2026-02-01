using MatchFixer.Infrastructure.Models.TheSportsDBAPI;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface ITheSportsDbApiService
	{
		Task<List<LeagueTableRowApiDto>> GetLeagueTableAsync(
			int leagueId,
			string season,
			CancellationToken ct = default);
	}
}
