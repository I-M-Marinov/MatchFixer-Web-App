using MatchFixer.Core.ViewModels.MatchGuessGame;
using MatchFixer.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class GameController : Controller
	{
		private readonly IMatchGuessGameService _gameService;

		public GameController(IMatchGuessGameService gameService)
		{
			_gameService = gameService;
		}

		[HttpGet]
		public async Task<IActionResult> Start(int question = 1, int score = 0, int total = 5)
		{
			var match = await _gameService.GetRandomMatchAsync();
			if (match == null) return View("GameOver", score);

			var matchViewModel = new MatchGuessGameViewModel
			{
				QuestionNumber = question,
				TotalQuestions = total,
				Score = score,
				MatchId = match.Id,
				HomeTeam = match.HomeTeam,
				AwayTeam = match.AwayTeam,
				HomeTeamLogo = match.HomeTeamLogo,
				AwayTeamLogo = match.AwayTeamLogo
			};

			return View("Play", matchViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Submit(MatchGuessGameViewModel model)
		{
			var match = await _gameService.GetMatchByIdAsync(model.MatchId);
			if (match == null) return RedirectToAction("Start");

			bool isCorrect = _gameService.CheckAnswer(match, model.UserHomeGuess ?? -1, model.UserAwayGuess ?? -1);
			model.IsCorrect = isCorrect;
			model.Score += isCorrect ? 10 : 0;
			model.ActualHomeScore = match.HomeScore;
			model.ActualAwayScore = match.AwayScore;
			model.IsAnswered = true;
			model.HomeTeam = match.HomeTeam;
			model.AwayTeam = match.AwayTeam;
			model.HomeTeamLogo = match.HomeTeamLogo;
			model.AwayTeamLogo = match.AwayTeamLogo;

			if (model.QuestionNumber >= model.TotalQuestions)
				return View("GameOver", model.Score);

			//return RedirectToAction("Start", new { question = model.QuestionNumber + 1, score = model.Score, total = model.TotalQuestions });
			return View("Play", model);
		}
	}

}
