using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using MatchFixer.Core.Contracts;
using MatchFixer.Core.Extensions;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Core.ViewModels.GameSessionState;
using MatchFixer.Core.ViewModels.MatchGuessGame;
using MatchFixer.Infrastructure;

namespace MatchFixer.Core.Services
{
	public class MatchGuessGameService  : IMatchGuessGameService
	{
		private readonly MatchFixerDbContext _context;
		private readonly Random _random = new();

		public MatchGuessGameService(MatchFixerDbContext context)
		{
			_context = context;
		}

		public async Task<MatchResult?> GetRandomMatchAsync()
		{
			var totalMatches = await _context.MatchResults.CountAsync();
			if (totalMatches == 0) return null;

			int skip = _random.Next(0, totalMatches);
			return await _context.MatchResults.Skip(skip).Take(1).FirstOrDefaultAsync();
		}

		public bool CheckAnswer(MatchResult match, int userHomeScore, int userAwayScore)
		{
			return match.HomeScore == userHomeScore && match.AwayScore == userAwayScore;
		}
		public async Task<MatchResult?> GetMatchByIdAsync(int id)
		{
			return await _context.MatchResults.FindAsync(id);
		}

		public async Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> PrepareNextQuestionAsync(ISession session)
		{
			const string SessionKey = "GameSession";

			var sessionState = session.GetObject<GameSessionState>(SessionKey) ?? new GameSessionState();

			if (!sessionState.LastQuestionAnswered && sessionState.QuestionNumber > 1)
			{
				sessionState.QuestionNumber++;
			}

			if (sessionState.QuestionNumber > sessionState.TotalQuestions)
			{
				session.Remove(SessionKey);
				return (new MatchGuessGameViewModel
				{
					Score = sessionState.Score
				}, true); // Game over
			}

			sessionState.LastQuestionAnswered = false;
			session.SetObject(SessionKey, sessionState);

			var match = await GetRandomMatchAsync();
			if (match == null)
			{
				session.Remove(SessionKey);
				return (new MatchGuessGameViewModel
				{
					Score = sessionState.Score
				}, true); // game over
			}

			// Build the ViewModel
			var viewModel = new MatchGuessGameViewModel
			{
				QuestionNumber = sessionState.QuestionNumber,
				TotalQuestions = sessionState.TotalQuestions,
				Score = sessionState.Score,
				MatchId = match.Id,
				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,
				HomeTeamLogo = match.HomeTeamLogo,
				AwayTeamLogo = match.AwayTeamLogo
			};

			return (viewModel, false); // Game continues
		}

		public async Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> ProcessAnswerAsync(
			ISession session,
			MatchGuessGameViewModel submittedModel)
		{
			const string SessionKey = "GameSession";

			var sessionState = session.GetObject<GameSessionState>(SessionKey);
			if (sessionState == null)
			{
				return (null, true); // Session expired
			}

			var match = await GetMatchByIdAsync(submittedModel.MatchId);
			if (match == null)
			{
				return (null, true); // Match no longer available
			}

			// Evaluate the guess
			bool isCorrect = CheckAnswer(match, submittedModel.UserHomeGuess ?? -1, submittedModel.UserAwayGuess ?? -1);
			sessionState.Score += isCorrect ? 10 : 0;
			sessionState.LastQuestionAnswered = true;

			bool isGameOver = sessionState.QuestionNumber == sessionState.TotalQuestions;

			// Only increment if game is continuing
			if (!isGameOver)
			{
				sessionState.QuestionNumber++;
				session.SetObject(SessionKey, sessionState);
			}
			else
			{
				session.Remove(SessionKey);
			}

			var resultViewModel = new MatchGuessGameViewModel
			{
				IsCorrect = isCorrect,
				Score = sessionState.Score,
				QuestionNumber = sessionState.QuestionNumber - (isGameOver ? 0 : 1), // show current, not next
				TotalQuestions = sessionState.TotalQuestions,
				MatchId = match.Id,
				ActualHomeScore = match.HomeScore,
				ActualAwayScore = match.AwayScore,
				IsAnswered = true,
				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,
				HomeTeamLogo = match.HomeTeamLogo,
				AwayTeamLogo = match.AwayTeamLogo
			};

			return (resultViewModel, isGameOver);
		}
	}
}
