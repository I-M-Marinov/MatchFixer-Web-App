using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class LogoQuizController : Controller
	{
		private readonly ILogoQuizService _logoQuizService;
		private readonly IUserContextService _userContextService;

		public LogoQuizController(ILogoQuizService logoQuizService, IUserContextService userContextService)
		{
			_logoQuizService = logoQuizService;
			_userContextService = userContextService;
		}

		[HttpGet]
		public async Task<IActionResult> LogoQuiz(int currentScore = 0)
		{
			var question = await _logoQuizService.GenerateQuestionAsync(currentScore);
			return View(question);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogoQuiz(string SelectedAnswer, string CorrectAnswer, string LogoUrl, List<string> OriginalOptions)
		{
			var userId = _userContextService.GetUserId();
			var model = _logoQuizService.BuildAnsweredModel(SelectedAnswer, CorrectAnswer, LogoUrl, OriginalOptions);

			var (pointsMessage, updatedScore) = await _logoQuizService.UpdateLogoQuizScoreAsync(userId, model.IsCorrect == true);
			model.CurrentScore = updatedScore;

			if (model.IsCorrect == true)
			{
				TempData["SuccessMessage"] = pointsMessage; // Store success message
			}
			else
			{
				TempData["ErrorMessage"] = pointsMessage; // Store error or no-change
			}

			return View(model);
		}
	}
}
