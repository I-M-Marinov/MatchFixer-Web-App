using MatchFixer.Core.Contracts;
using MatchFixer.Core.Services;
using MatchFixer.Core.ViewModels.MatchResults;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer.Infrastructure.Contracts;
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
		private readonly ISessionService _sessionService;
		private readonly ITimezoneService _timezoneService;

		public ResultController(ILiveMatchResultService resultService, IEventsResultsService eventsResultsService, ISessionService sessionService, ITimezoneService timezoneService)
		{
			_resultService = resultService;
			_eventsResultsService = eventsResultsService;
			_sessionService = sessionService;
			_timezoneService = timezoneService;
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
		public async Task<IActionResult> EventsResults(int day = 0)
		{
			var model = await _eventsResultsService.GetByDayAsync(day);

			var userTimeZoneId = _sessionService.GetUserTimezone(); 
			var userNow = _timezoneService.ConvertToUserTime(DateTime.UtcNow, userTimeZoneId)!;

			model.TodayLocal = DateOnly.FromDateTime(userNow.Value);

			return View(model);
		}


	}
}
