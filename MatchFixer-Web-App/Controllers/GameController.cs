using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchGuessGame;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class GameController : Controller
	{
		private readonly IMatchGuessGameService _gameService;
		private const string SessionKey = "GameSession";

		public GameController(IMatchGuessGameService gameService)
		{
			_gameService = gameService;
		}

		[HttpGet]
		public async Task<IActionResult> Start()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
			Response.Headers["Pragma"] = "no-cache";
			Response.Headers["Expires"] = "0";

			var (viewModel, isGameOver) = await _gameService.PrepareNextQuestionAsync(HttpContext.Session);

			if (isGameOver)
				return View("GameOver", viewModel.Score);

			return View("Play", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Submit(MatchGuessGameViewModel model)
		{
			var (viewModel, isGameOver) = await _gameService.ProcessAnswerAsync(HttpContext.Session, model);

			if (isGameOver || viewModel == null)
				return View("GameOver", viewModel?.Score ?? 0);

			return View("Play", viewModel);
		}


		[HttpGet]
		public IActionResult Reset()
		{
			HttpContext.Session.Remove(SessionKey);
			return RedirectToAction("Start");
		}
	}

}
