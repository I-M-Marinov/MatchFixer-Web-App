using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchResults;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.MatchResultConstants;

namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	public class ResultController : Controller
	{
		private readonly ILiveMatchResultService _resultService;
		private readonly IEventsResultsService _eventsResultsService;

		public ResultController(ILiveMatchResultService resultService, IEventsResultsService eventsResultsService)
		{
			_resultService = resultService;
			_eventsResultsService = eventsResultsService;

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
				TempData["SuccessMessage"] = MatchResultSavedSuccessfully;
			else
				TempData["ErrorMessage"] = MatchResultSaveFailed;

			return RedirectToAction(nameof(LiveMatchResults));
		}

		[HttpGet]
		public async Task<IActionResult> EventsResults(int count = 10)
		{
			var model = await _eventsResultsService.GetLatestAsync(count);
			return View(model);
		}


	}
}
