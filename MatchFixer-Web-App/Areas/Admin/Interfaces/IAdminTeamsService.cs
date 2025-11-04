using MatchFixer_Web_App.Areas.Admin.ViewModels.Teams;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminTeamsService
	{
		Task<IReadOnlyList<TeamSearchResult>> SearchTeamsAsync(string name, CancellationToken ct = default);
		Task<bool> AddTeamFromSearchAsync(int apiTeamId, string name, string logoUrl, int leagueId, CancellationToken ct = default);
		Task<PaginatedTeamsList<TeamListRow>> GetTeamsPageAsync(int page, int pageSize, CancellationToken ct = default);
	}
}
