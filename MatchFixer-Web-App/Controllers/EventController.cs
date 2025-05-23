using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class EventController : Controller
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IMatchEventService _matchEventService;

		public EventController(MatchFixerDbContext dbContext, IMatchEventService matchEventService)
		{
			_dbContext = dbContext;
			_matchEventService = matchEventService;
		}

		[HttpGet]
		[Authorize]
		public IActionResult AddMatchEvent()
		{
			return View();
		}

		[HttpPost]
		[Authorize]

		public async Task<IActionResult> AddMatchEvent(MatchEventFormModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				await _matchEventService.AddEventAsync(model);
			}
			catch (Exception e)
			{
				TempData["ErrorMessage"] = e.Message;
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
