using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.LogoQuizConstants;

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

		[HttpGet("/games/logo-quiz")]
		public async Task<IActionResult> LogoQuiz(int currentScore = 0, bool skip = false)
		{
			var user = await _userContextService.GetCurrentUserAsync();

			if (skip)
			{
				currentScore = await _logoQuizService.DeductSkipPenaltyAsync(user.Id);
				TempData["ErrorMessage"] = SkippedQuestion;
			}
			else
			{
				currentScore = user.LogoQuizScore;
			}

			var question = await _logoQuizService.GenerateQuestionAsync(currentScore);
			return View(question);
		}

		[HttpPost("/games/logo-quiz")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogoQuiz(string SelectedAnswer, string CorrectAnswer, string LogoUrl, List<string> OriginalOptions)
		{
			var userId = _userContextService.GetUserId();
			var model = _logoQuizService.BuildAnsweredModel(SelectedAnswer, CorrectAnswer, LogoUrl, OriginalOptions);

			var (pointsMessage, updatedScore) = await _logoQuizService.UpdateLogoQuizScoreAsync(userId, model.IsCorrect == true);
			model.CurrentScore = updatedScore;

			TempData[model.IsCorrect == true
				? "SuccessMessage"
				: "ErrorMessage"] = pointsMessage;

			return View(model);
		}
	}
}
