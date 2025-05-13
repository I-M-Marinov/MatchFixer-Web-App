using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchGuessGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class GameController : Controller
	{
		private readonly IMatchGuessGameService _gameService;
		private readonly IMatchFixScoreService _scoreService;
		private readonly ISessionService _sessionService;


		public GameController(IMatchGuessGameService gameService, IMatchFixScoreService scoreService, ISessionService sessionService)
		{
			_gameService = gameService;
			_scoreService = scoreService;
			_sessionService = sessionService;
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Landing()
		{
			var userId = HttpContext.Session.GetString("UserId"); // get the userId from the session 
			Console.WriteLine($"USER ID ________________________________________________________ {userId}");
			_sessionService.InitializeSessionState(userId); // use the userId to initialize the game session

			var leaderboard = await _scoreService.GetTopPlayersAsync();
			return View("GameStartScreen", leaderboard);
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Start()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
			Response.Headers["Pragma"] = "no-cache";
			Response.Headers["Expires"] = "0";

			var (viewModel, isGameOver) = await _gameService.PrepareNextQuestionAsync(HttpContext.Session);

			// for debugging purposes only ( and honest cheating  :D ) 
			Console.WriteLine($"{viewModel.ActualHomeScore} : {viewModel.ActualAwayScore}");
			Console.WriteLine($" Score ---------------->>>>>> {viewModel.Score}");

			if (isGameOver) return View("GameOver", viewModel.Score);

			return View("Play", viewModel);
		}

		[Authorize]
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
			// Clear the old GameSession
			_sessionService.ClearSession();
			// Re-initialize the GameSession state 
			var userId = HttpContext.Session.GetString("UserId"); // get the userId from the session 
			_sessionService.InitializeSessionState(userId); // use the userId to initialize the game session
			return RedirectToAction("Start");
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Leaderboard()
		{
			var leaderboard = await _scoreService.GetTopPlayersAsync();

			return View("Leaderboard", leaderboard);
		}
	}

}
