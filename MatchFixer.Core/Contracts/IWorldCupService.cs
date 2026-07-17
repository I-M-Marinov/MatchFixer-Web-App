using MatchFixer.Core.ViewModels.WordCup;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;

namespace MatchFixer.Core.Contracts
{
	public interface IWorldCupService
	{
		Task<WorldCupPageViewModel> GetWorldCupPageAsync();
		Task<List<WorldCupGroupStandingViewModel>>
			GetGroupStandingsAsync();
		Task<int> RefreshKnockoutStageAsync();
		Task<int> ReclassifyAndRefreshAsync();
		Task<int> RefreshGroupStandingsAsync();

		/// <summary>Returns all knockout matches ordered by Stage then RoundPosition.</summary>
		Task<List<BracketMatchOrderDto>> GetKnockoutMatchOrderAsync();

		/// <summary>Bulk-updates RoundPosition for the given match IDs.</summary>
		Task SaveBracketOrderAsync(IEnumerable<(int Id, int Position)> order);
	}
}
