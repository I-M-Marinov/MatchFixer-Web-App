using MatchFixer.Common.Enums;
using MatchFixer.Core.ViewModels.LeagueTables;

namespace MatchFixer.Core.Contracts
{
	public interface ILeagueTableService
	{
		Task<List<LeagueTableRowViewModel>> GetLeagueTableAsync(
			InternalLeague league,
			string? season = null,
			CancellationToken ct = default);
	}
}
