﻿using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchResults;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.MatchResultConstants;

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
				TempData["SuccessMessage"] = MatchResultSavedSuccessfully;
			else
				TempData["ErrorMessage"] = MatchResultSaveFailed;

			return RedirectToAction(nameof(LiveMatchResults));
		}


	}
}
