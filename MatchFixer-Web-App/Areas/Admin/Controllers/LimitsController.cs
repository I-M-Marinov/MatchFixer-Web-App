using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Limits;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Limits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	/// <summary>Admin "Limits" page: market switches, stake limits by league x legs, max win per league.</summary>
	[Area("Admin")]
	[AdminOnly]
	public class LimitsController : Controller
	{
		private readonly IBettingLimitsService _limits;
		private readonly IUserContextService _userContext;
		private readonly MatchFixerDbContext _db;

		public LimitsController(IBettingLimitsService limits, IUserContextService userContext, MatchFixerDbContext db)
		{
			_limits = limits;
			_userContext = userContext;
			_db = db;
		}

		[HttpGet("/admin/limits", Name = "AdminLimits")]
		public async Task<IActionResult> Index(CancellationToken ct)
		{
			var config = await _limits.GetConfigAsync(ct);

			var adminIds = config.Leagues
				.Select(l => l.UpdatedByUserId)
				.Append(config.MarketsUpdatedByUserId)
				.Where(id => id.HasValue)
				.Select(id => id!.Value)
				.Distinct()
				.ToList();

			var names = await _db.Users
				.AsNoTracking()
				.Where(u => adminIds.Contains(u.Id))
				.Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
				.ToListAsync(ct);

			var vm = new LimitsPageViewModel
			{
				Config = config,
				AvailableLeagues = await _limits.GetAvailableLeaguesAsync(ct),
				AdminNames = names.ToDictionary(
					u => u.Id,
					u => string.IsNullOrWhiteSpace($"{u.FirstName} {u.LastName}".Trim()) ? (u.UserName ?? "an admin") : $"{u.FirstName} {u.LastName}".Trim())
			};

			return View(vm);
		}

		/// <summary>Saves the whole page (sent as JSON so decimals are read the same way on every server culture).</summary>
		[HttpPost("/admin/limits/save")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Save([FromBody] BettingLimitsConfigDto model, CancellationToken ct)
		{
			if (model == null)
				return BadRequest(new { success = false, message = "Nothing to save." });

			var error = await _limits.SaveConfigAsync(model, _userContext.GetUserId(), ct);

			return error == null
				? Json(new { success = true, message = "Limits saved." })
				: BadRequest(new { success = false, message = error });
		}
	}
}
