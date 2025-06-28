using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class LogoQuizController : Controller
	{
		private readonly ILogoQuizService _logoQuizService;

		public LogoQuizController(ILogoQuizService logoQuizService)
		{
			_logoQuizService = logoQuizService;
		}

		[HttpGet]
		public async Task<IActionResult> LogoQuiz()
		{
			var question = await _logoQuizService.GenerateQuestionAsync();
			return View(question);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult LogoQuiz(string SelectedAnswer, string CorrectAnswer, string LogoUrl, List<string> OriginalOptions)
		{
			var model = _logoQuizService.BuildAnsweredModel(SelectedAnswer, CorrectAnswer, LogoUrl, OriginalOptions);
			return View(model);
		}
	}
}
