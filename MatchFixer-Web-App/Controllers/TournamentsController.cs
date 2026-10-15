using MatchFixer.Common.GeneralConstants;
using MatchFixer.Common.Identity;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Tournaments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	public class TournamentsController : Controller
	{
		private readonly ITournamentService _tournaments;
		private readonly IUserContextService _userContext;

		public TournamentsController(ITournamentService tournaments, IUserContextService userContext)
		{
			_tournaments = tournaments;
			_userContext = userContext;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var userId = _userContext.GetUserId();
			var isAdmin = User.IsInRole(Roles.Admin);
			var model = await _tournaments.GetListAsync(userId, isAdmin);
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Details(Guid id)
		{
			var userId = _userContext.GetUserId();
			var isAdmin = User.IsInRole(Roles.Admin);

			var model = await _tournaments.GetDetailsAsync(id, userId, isAdmin);
			if (model == null)
			{
				TempData[TempDataKeys.ErrorMessage] = "Tournament not found.";
				return RedirectToAction(nameof(Index));
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateTournamentInputModel input)
		{
			var userId = _userContext.GetUserId();
			var isAdmin = User.IsInRole(Roles.Admin);

			if (!ModelState.IsValid)
			{
				TempData[TempDataKeys.ErrorMessage] = "Please check the tournament details and try again.";
				return RedirectToAction(nameof(Index));
			}

			var (success, message, tournamentId) = await _tournaments.CreateAsync(userId, input, isAdmin);
			if (!success || tournamentId == null)
			{
				TempData[TempDataKeys.ErrorMessage] = message;
				return RedirectToAction(nameof(Index));
			}

			TempData[TempDataKeys.SuccessMessage] = message;
			return RedirectToAction(nameof(Details), new { id = tournamentId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Join(Guid id)
		{
			var userId = _userContext.GetUserId();
			var (success, message) = await _tournaments.JoinAsync(id, userId);
			TempData[success ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = message;
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceBet(Guid id, Guid[] matchIds, string[] picks, decimal stake)
		{
			var userId = _userContext.GetUserId();

			var legs = new List<(Guid MatchId, string Pick)>();
			if (matchIds != null && picks != null)
			{
				var n = Math.Min(matchIds.Length, picks.Length);
				for (var i = 0; i < n; i++)
					legs.Add((matchIds[i], picks[i]));
			}

			var (success, message) = await _tournaments.PlaceBetAsync(id, userId, legs, stake);
			TempData[success ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = message;
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PostMessage(Guid id, string text)
		{
			var userId = _userContext.GetUserId();
			var (success, message) = await _tournaments.PostMessageAsync(id, userId, text);
			if (!success)
				TempData[TempDataKeys.ErrorMessage] = message;
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Cancel(Guid id)
		{
			var (success, message) = await _tournaments.CancelAsync(id);
			TempData[success ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = message;
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Settle(Guid id)
		{
			var (success, message) = await _tournaments.SettleAsync(id);
			TempData[success ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = message;
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
