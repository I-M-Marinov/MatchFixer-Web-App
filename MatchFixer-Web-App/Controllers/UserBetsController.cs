using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	public class UserBetsController : Controller
	{

		private readonly IBettingService _bettingService;

		public UserBetsController(IBettingService bettingService)
		{
			_bettingService = bettingService;
		}

		public async Task<IActionResult> UserBets()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userIdString, out var userId))
			{
				return Unauthorized();
			}

			var userBets = await _bettingService.GetBetsByUserAsync(userId);

			return View(userBets);
		}
		
	}
}
