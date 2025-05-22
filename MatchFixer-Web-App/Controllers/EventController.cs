using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Controllers
{
	public class EventController : Controller
	{
		private readonly MatchFixerDbContext _dbContext;

		public EventController(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
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

			var matchEvent = new MatchEvent
			{
				Id = Guid.NewGuid(),
				HomeTeam = model.HomeTeam,
				AwayTeam = model.AwayTeam,
				MatchDate = model.MatchDate,
				HomeOdds = model.HomeOdds,
				DrawOdds = model.DrawOdds,
				AwayOdds = model.AwayOdds
			};

			_dbContext.MatchEvents.Add(matchEvent);
			await _dbContext.SaveChangesAsync();

			return RedirectToAction(nameof(LiveEvents));
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> LiveEvents()
		{
			var events =  await _dbContext.MatchEvents
				.OrderBy(e => e.MatchDate)
				.Select(e => new LiveEventViewModel
				{
					Id = e.Id,
					HomeTeam = e.HomeTeam,
					AwayTeam = e.AwayTeam,
					KickoffTime = e.MatchDate,
					HomeWinOdds = e.HomeOdds ?? 0,
					DrawOdds = e.DrawOdds ?? 0,
					AwayWinOdds = e.AwayOdds ?? 0
				})
				.ToListAsync();

			return View(events);
		}
	}
}
