using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.DTO;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure.Models.FootballAPI;
using MatchFixer.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MatchFixer.Common.GeneralConstants.MatchEventConstants;
using static MatchFixer.Common.GeneralConstants.OddsBoostConstants;
using static MatchFixer.Common.GeneralConstants.MatchEventsApiConstants;

namespace MatchFixer_Web_App.Controllers
{
	public class EventController : Controller
	{
		private readonly IMatchEventService _matchEventService;
		private readonly IUpcomingMatchService _upcomingMatchService;
		private readonly IUserContextService _userContextService;
		private readonly IOddsBoostService _oddsBoostService;

		public EventController(IMatchEventService matchEventService, IUpcomingMatchService upcomingMatchService, IUserContextService userContextService, IOddsBoostService oddsBoostService)
		{
			_matchEventService = matchEventService;
			_upcomingMatchService = upcomingMatchService;
			_userContextService = userContextService;
			_oddsBoostService = oddsBoostService;
		}

		[HttpGet]
		[Authorize]
		[AdminOnly]
		public async Task<IActionResult> AddMatchEvent()
		{
			var model = new MatchEventFormModel
			{
				TeamsByLeague = await _matchEventService.GetTeamsGroupedByLeagueAsync(),
				CurrentEvents = await _matchEventService.GetAllEventsAsync(),
				ApiLeagues = await _matchEventService.GetApiLeaguesAsync()
			};

			return View(model);
		}

		[HttpPost]
		[Authorize]
		[AdminOnly]
		[ValidateAntiForgeryToken]
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

		[Authorize]
		[AdminOnly]
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

		[Authorize]
		[AdminOnly]
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

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken] 
		public async Task<IActionResult> GetLatestOdds([FromBody] Guid[] matchIds)
		{
			var odds = await _matchEventService.GetOddsForMatchesAsync(matchIds);
			return Json(odds);
		}

		[AdminOnly]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateOddsBoost(CreateOddsBoostViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var user = await _userContextService.GetCurrentUserAsync();

				DateTime? startUtc = null;

				if (model.StartUtc.HasValue)
				{
					startUtc = model.StartUtc.Value.ToUniversalTime();
				}

				var boost = await _oddsBoostService.CreateOddsBoostAsync(
					matchEventId: model.MatchEventId,
					boostValue: model.BoostValue,
					duration: TimeSpan.FromMinutes(model.DurationMinutes),
					createdByUserId: user.Id,
					startUtc: startUtc, 
					maxStakePerBet: model.MaxStakePerBet,
					maxUsesPerUser: model.MaxUsesPerUser,
					note: model.Note);

				return Json(new { success = true, message = OddsBoostCreatedSuccessfully });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpGet]
		[Authorize]
		[AdminOnly]
		public async Task<IActionResult> UpcomingFromApi(int leagueId)
		{
			var matches = await _upcomingMatchService.GetUpcomingMatchesAsync(leagueId);

			var model = new UpcomingMatchesSelectionViewModel
			{
				Selected = matches
			};

			return PartialView("_UpcomingApiMatches", model);
		}
		[HttpPost]
		[Authorize]
		[AdminOnly]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddUpcomingMatches(
			UpcomingMatchesSelectionViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage);
				TempData["ErrorMessage"] = string.Join(", ", errors);
				return RedirectToAction(nameof(AddMatchEvent));
			}
			if (!model.Selected.Any(x => x.Selected))
			{
				TempData["ErrorMessage"] = NoMatchesWereSelected;
				return RedirectToAction(nameof(AddMatchEvent));
			}
			foreach (var match in model.Selected.Where(x => x.Selected))
				await _matchEventService.AddEventFromUpcomingAsync(match);

			if (model.Selected.Count > 1 && model.Selected.Count != 0)
			{
				TempData["SuccessMessage"] = MatchEventsWereAddedSuccessfully;
			}
			else
			{
				TempData["SuccessMessage"] = MatchEventWasAddedSuccessfully;
			}

			return RedirectToAction(nameof(AddMatchEvent));
		}

		[HttpPost]
		[Authorize]
		[AdminOnly]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StopOddsBoost([FromBody] StopOddsBoostRequest model)
		{
			if (model == null || model.OddsBoostId == Guid.Empty)
				return BadRequest("Invalid boost id.");

			try
			{
				var user = await _userContextService.GetCurrentUserAsync();

				await _oddsBoostService.StopOddsBoostAsync(
					model.OddsBoostId,
					user.Id
				);

				return Ok(new { success = true });
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize]
		[AdminOnly]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PostponeMatchEvent(Guid id)
		{
			try
			{
				var user = await _userContextService.GetCurrentUserAsync();
				await _matchEventService.PostponeMatchAsync(id, user.Id);

				TempData["SuccessMessage"] = MatchHasBeenPostponed;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}

			return RedirectToAction(nameof(AddMatchEvent));
		}



	}
}
