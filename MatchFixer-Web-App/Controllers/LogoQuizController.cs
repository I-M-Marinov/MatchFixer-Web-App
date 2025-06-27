using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LogoQuiz;
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
		public async Task<IActionResult> LogoQuiz(LogoQuizQuestionViewModel model)
		{
			model.IsCorrect = model.SelectedAnswer == model.CorrectAnswer;
			return View(model); // Show feedback
		}
	}
}
