using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
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
				TeamsByLeague = await _matchEventService.GetTeamsGroupedByLeagueAsync()
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

			return RedirectToAction(nameof(LiveEvents));
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
	}
}
