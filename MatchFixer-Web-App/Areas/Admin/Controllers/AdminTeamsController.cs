using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.Services;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Teams;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]

	public class AdminTeamsController : Controller
	{
		private readonly IAdminTeamsService _svc;
		private const int DefaultPageSize = 10;


		public AdminTeamsController(IAdminTeamsService svc)
		{
			_svc = svc;
		}

		[HttpGet]
		public async Task<IActionResult> TeamsIndex(int page = 1, int pageSize = DefaultPageSize, CancellationToken ct = default)
		{
			var leagues = AdminTeamsService.LeagueMap;
			var searchVm = new TeamSearchVm { Leagues = leagues, Results = Array.Empty<TeamSearchResult>() };
			var existing = await _svc.GetTeamsPageAsync(page, pageSize, ct);

			var model = Tuple.Create(searchVm, existing);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Search(TeamSearchVm searchVm, CancellationToken ct)
		{
			searchVm.Leagues = AdminTeamsService.LeagueMap;
			searchVm.Results = string.IsNullOrWhiteSpace(searchVm.Query)
				? Array.Empty<TeamSearchResult>()
				: await _svc.SearchTeamsAsync(searchVm.Query!, ct);

			var existing = await _svc.GetTeamsPageAsync(1, DefaultPageSize, ct);

			var model = Tuple.Create(searchVm, existing);
			return View("TeamsIndex", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(int apiTeamId, int leagueId, string name, string logoUrl, CancellationToken ct)
		{
			var ok = await _svc.AddTeamFromSearchAsync(apiTeamId, name, logoUrl, leagueId, ct);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Team added." : "Already exists.";

			return RedirectToAction(nameof(TeamsIndex));
		}
	}

}
