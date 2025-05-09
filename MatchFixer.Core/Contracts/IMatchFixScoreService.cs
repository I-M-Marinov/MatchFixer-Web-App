
using MatchFixer.Core.ViewModels.MatchGuessGame;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchFixScoreService
	{
		Task<List<LeaderboardEntryViewModel>> GetTopPlayersAsync(int count = 10);

	}
}
