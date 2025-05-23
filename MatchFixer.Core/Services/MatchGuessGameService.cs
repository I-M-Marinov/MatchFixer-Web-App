using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Core.ViewModels.MatchGuessGame;
using MatchFixer.Infrastructure;



namespace MatchFixer.Core.Services
{
	public class MatchGuessGameService  : IMatchGuessGameService
	{
		private readonly MatchFixerDbContext _context;
		private readonly Random _random = new();
		private readonly ISessionService _sessionService;


		public MatchGuessGameService(MatchFixerDbContext context, ISessionService sessionService)
		{
			_context = context;
			_sessionService = sessionService;
		}

		public async Task<MatchResult?> GetRandomMatchAsync()
		{
			var ids = await _context.MatchResults
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.AsNoTracking()
				.Select(m => m.Id).ToListAsync();

			if (!ids.Any()) return null;

			int randomId = ids[_random.Next(ids.Count)];

			var randomGame = await _context.MatchResults
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == randomId);

			return randomGame;
		}

		public bool CheckAnswer(MatchResult match, int userHomeScore, int userAwayScore)
		{
			return match.HomeScore == userHomeScore && match.AwayScore == userAwayScore;
		}
		public async Task<MatchResult?> GetMatchByIdAsync(int id)
		{
			return await _context.MatchResults
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.Where(m => m.Id == id)
				.AsNoTracking()
				.FirstOrDefaultAsync();
		}

		public async Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> PrepareNextQuestionAsync(ISession session)
		{

			var sessionState = _sessionService.GetSessionState();

			if (!sessionState.LastQuestionAnswered && sessionState.QuestionNumber > 1)
			{
				sessionState.QuestionNumber++;
			}

			if (sessionState.QuestionNumber > sessionState.TotalQuestions)
			{
				_sessionService.ClearSession();

				return (new MatchGuessGameViewModel
				{
					Score = sessionState.Score
				}, true); // Game over
			}

			sessionState.LastQuestionAnswered = false;

			_sessionService.SetSessionState(sessionState);

			var match = await GetRandomMatchAsync();

			if (match == null)
			{
				_sessionService.ClearSession();

				return (new MatchGuessGameViewModel
				{
					Score = sessionState.Score
				}, true); // Game over
			}

			// Build the ViewModel
			var viewModel = new MatchGuessGameViewModel
			{
				QuestionNumber = sessionState.QuestionNumber,
				TotalQuestions = sessionState.TotalQuestions,
				Score = sessionState.Score,
				MatchId = match.Id,
				League = match.LeagueName,
				ActualHomeScore = match.HomeScore,
				ActualAwayScore = match.AwayScore,
				HomeTeam = match.HomeTeam.Name,
				AwayTeam = match.AwayTeam.Name,
				HomeTeamLogo = match.HomeTeam.LogoUrl,
				AwayTeamLogo = match.AwayTeam.LogoUrl
			};

			return (viewModel, false); // Game continues
		}

		public async Task<(MatchGuessGameViewModel ViewModel, bool IsGameOver)> ProcessAnswerAsync(
	ISession session,
	MatchGuessGameViewModel submittedModel)
		{
			var sessionState = _sessionService.GetSessionState();

			if (sessionState == null)
			{
				return (null, true); // Session expired
			}

			var match = await GetMatchByIdAsync(submittedModel.MatchId);
			if (match == null)
			{
				return (null, true); // Match no longer available
			}

			bool isCorrect = CheckAnswer(match, submittedModel.UserHomeGuess ?? -1, submittedModel.UserAwayGuess ?? -1);

			bool hitFifty = sessionState.Score == 40 && isCorrect;

			int pointsToAdd = 0;

			if (isCorrect)
			{
				// Base points: 10 or 20
				pointsToAdd = sessionState.Score >= 50 ? 20 : 10;

				// Add base points to session
				sessionState.Score += pointsToAdd;

				// Apply 50-point doubling bonus if just hit 50
				if (hitFifty && sessionState.Score == 50)
				{
					sessionState.Score *= 2; // becomes 100
					pointsToAdd += 50; // Also add the bonus to DB
				}

				// Update DB score
				await UpdateUserScoreAsync(Guid.Parse(sessionState.UserId), pointsToAdd);
			}

			sessionState.LastQuestionAnswered = true;
			bool isGameOver = sessionState.QuestionNumber == sessionState.TotalQuestions;

			if (!isGameOver)
			{
				sessionState.QuestionNumber++;
				_sessionService.SetSessionState(sessionState);
			}
			else
			{
				_sessionService.ClearSession();
			}

			var resultViewModel = new MatchGuessGameViewModel
			{
				IsCorrect = isCorrect,
				Score = sessionState.Score,
				QuestionNumber = sessionState.QuestionNumber - (isGameOver ? 0 : 1),
				TotalQuestions = sessionState.TotalQuestions,
				MatchId = match.Id,
				League = match.LeagueName,
				ActualHomeScore = match.HomeScore,
				ActualAwayScore = match.AwayScore,
				IsAnswered = true,
				HomeTeam = match.HomeTeam.Name,
				AwayTeam = match.AwayTeam.Name,
				HomeTeamLogo = match.HomeTeam.LogoUrl,
				AwayTeamLogo = match.AwayTeam.LogoUrl
			};

			return (resultViewModel, isGameOver);
		}

		private async Task UpdateUserScoreAsync(Guid userId, int score)
		{
			var user = await _context.ApplicationUsers.FindAsync(userId);
			if (user != null)
			{
				user.MatchFixScore += score;
				await _context.SaveChangesAsync();
			}
		}

	}
}
