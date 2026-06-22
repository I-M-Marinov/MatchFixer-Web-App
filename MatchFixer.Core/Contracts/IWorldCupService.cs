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
	}
}
