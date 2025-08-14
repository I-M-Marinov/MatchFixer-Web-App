using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.MatchEventConstants;

namespace MatchFixer_Web_App.Controllers
{
	public class EventController : Controller
	{
		private readonly IMatchEventService _matchEventService;

		public EventController(IMatchEventService matchEventService)
		{
			_matchEventService = matchEventService;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> AddMatchEvent()
		{
			var model = new MatchEventFormModel
			{
				TeamsByLeague = await _matchEventService.GetTeamsGroupedByLeagueAsync(),
				CurrentEvents = await _matchEventService.GetAllEventsAsync()

			};

			return View(model);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddMatchEvent(MatchEventFormModel model)
		{
			if (!ModelState.IsValid)
			{
				model.TeamsByLeague = await _matchEventService.GetTeamsGroupedByLeagueAsync();
				return View(model);
			}

			try
			{
				await _matchEventService.AddEventAsync(model);
			}
			catch (Exception e)
			{
				TempData["ErrorMessage"] = e.Message;

				model.TeamsByLeague = await _matchEventService.GetTeamsGroupedByLeagueAsync();
				return View(model);
			}

			return RedirectToAction(nameof(AddMatchEvent));
		}


		[HttpGet]
		[Authorize]
		public async Task<IActionResult> LiveEvents()
		{
			var events = new List<LiveEventViewModel>();

			try
			{
				events = await _matchEventService.GetLiveEventsAsync();
			}
			catch (Exception e)
			{
				TempData["ErrorMessage"] = FailedToGetLiveEvents;
			}

			return View(events);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditMatchEvent(Guid id, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime matchDate)
		{
			try
			{
				var result = await _matchEventService.EditMatchEventAsync(id, homeOdds, drawOdds, awayOdds, matchDate);

				if (!result)
				{
					return RedirectToAction(nameof(AddMatchEvent));
				}
			}
			catch (Exception e)
			{
				TempData["ErrorMessage"] = e.Message;
				return RedirectToAction(nameof(AddMatchEvent));
			}

			TempData["SuccessMessage"] = MatchUpdateSuccessful;
			return RedirectToAction(nameof(AddMatchEvent));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelMatchEvent(Guid id)
		{
			var result = await _matchEventService.CancelMatchEventAsync(id);

			if (!result)
			{
				TempData["ErrorMessage"] = EventCancellationUnsuccessful;
				return RedirectToAction(nameof(AddMatchEvent));
			}

			TempData["SuccessMessage"] = EventCancellationSuccessful;
			return RedirectToAction(nameof(AddMatchEvent));
		}

		[HttpPost]
		[ValidateAntiForgeryToken] 
		public async Task<IActionResult> GetLatestOdds([FromBody] Guid[] matchIds)
		{
			var odds = await _matchEventService.GetOddsForMatchesAsync(matchIds);
			return Json(odds);
		}


	}
}
