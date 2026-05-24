using MatchFixer.Core.ViewModels.Index;

namespace MatchFixer.Core.Contracts
{
	public interface IHomePageService
	{
		Task<List<RecentBigWinViewModel>> GetRecentBigWinsAsync();
	}
}
