using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class LeagueTableController : Controller
	{
		private readonly ILeagueTableService _leagueTableService;

		public LeagueTableController(ILeagueTableService leagueTableServicebles)
		{
			_leagueTableService = leagueTableServicebles;
		}

		public async Task<IActionResult> LeagueTables(
			InternalLeague? league,
			string? season)
		{
			if (league is null)
			{
				league = InternalLeague.PremierLeague;
			}

			if (league.HasValue)
			{
				var table = await _leagueTableService
					.GetLeagueTableAsync(league.Value, season);

				ViewData["SelectedLeague"] = league.Value;
				ViewData["SelectedCompetition"] = null;

				return View("LeagueTables", table);
			}


			return RedirectToAction(nameof(LeagueTables));
		}
	}
}

