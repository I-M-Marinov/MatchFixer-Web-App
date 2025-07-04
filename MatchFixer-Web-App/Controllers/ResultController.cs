using MatchFixer.Core.Contracts;
using MatchFixer.Core.Services;
using MatchFixer.Core.ViewModels.MatchResults;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Controllers
{
	public class ResultController : Controller
	{
		private readonly ILiveMatchResultService _resultService;

		public ResultController(ILiveMatchResultService resultService)
		{
			_resultService = resultService;
		}

		[HttpGet]
		public async Task<IActionResult> LiveMatchResults()
		{
			var viewModel = await _resultService.GetUnresolvedMatchResultsAsync();
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitResult(MatchResultInputViewModel input)
		{
			if (!ModelState.IsValid)
			{
				var allMatches = await _resultService.GetUnresolvedMatchResultsAsync();

				var updatedList = allMatches.Select(m =>
					m.MatchId == input.MatchId ? input : m
				).ToList();

				return View("LiveMatchResults", updatedList); // Make sure view name matches
			}

			var success = await _resultService.AddMatchResultAsync(input.MatchId, input.HomeScore, input.AwayScore, input.Notes);

			if (success)
				TempData["SuccessMessage"] = "Match result saved successfully and winnings awarded.";
			else
				TempData["ErrorMessage"] = "Could not save result. It might already be recorded.";

			return RedirectToAction(nameof(LiveMatchResults));
		}


	}
}
