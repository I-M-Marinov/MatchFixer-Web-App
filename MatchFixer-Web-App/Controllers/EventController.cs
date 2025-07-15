using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
				TempData["ErrorMessage"] = "Failed to get the live events!"; 
			}

			return View(events);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditMatchEvent(Guid id, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime matchDate)
		{
			var result = await _matchEventService.EditMatchEventAsync(id, homeOdds, drawOdds, awayOdds, matchDate);

			if (!result)
			{
				TempData["Error"] = "Failed to edit the match. It might not exist or is already cancelled.";
				return RedirectToAction(nameof(AddMatchEvent));
			}

			TempData["Success"] = "Match updated successfully.";
			return RedirectToAction(nameof(AddMatchEvent));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelMatchEvent(Guid id)
		{
			var result = await _matchEventService.CancelMatchEventAsync(id);

			if (!result)
			{
				TempData["Error"] = "Could not cancel the event.";
				return RedirectToAction(nameof(AddMatchEvent));
			}

			TempData["Success"] = "Event cancelled.";
			return RedirectToAction(nameof(AddMatchEvent));
		}


	}
}
