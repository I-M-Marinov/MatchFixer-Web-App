using MatchFixer.Core.ViewModels.MatchGuessGame;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchGuessGameService
	{
		Task<MatchResult?> GetRandomMatchAsync();
		bool CheckAnswer(MatchResult match, int userHomeScore, int userAwayScore);
		Task<MatchResult?> GetMatchByIdAsync(int id);
		Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> PrepareNextQuestionAsync(ISession session);

		Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> ProcessAnswerAsync(ISession session,
			MatchGuessGameViewModel submittedModel);
	}
}
