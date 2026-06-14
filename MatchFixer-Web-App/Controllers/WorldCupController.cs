using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class WorldCupController : Controller
	{
		private readonly IWorldCupService _worldCupService;

		public WorldCupController(IWorldCupService worldCupService)
		{
			_worldCupService = worldCupService;
		}

		public async Task<IActionResult> WorldCup()
		{
			var model = await _worldCupService.GetWorldCupPageAsync();

			return View(model);
		}
	}
}
