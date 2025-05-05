using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface IMatchGuessGameService
	{
		Task<MatchResult?> GetRandomMatchAsync();
		bool CheckAnswer(MatchResult match, int userHomeScore, int userAwayScore);
		Task<MatchResult?> GetMatchByIdAsync(int id);
	}
}
