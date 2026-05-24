using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Index;
using MatchFixer_Web_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MatchFixer_Web_App.Controllers
{
	[AllowAnonymous]
	public class HomeController : Controller
    {
		private readonly ILogger<HomeController> _logger;
		private readonly IHomePageService _homePageService;

		public HomeController(
			ILogger<HomeController> logger,
			IHomePageService homePageService)
		{
			_logger = logger;
			_homePageService = homePageService;
		}

		public async Task<IActionResult> Index()
		{
	        if (User.Identity.IsAuthenticated) // if the user is already logged in redirect him to the profile page, not the home page 
	        {
				return RedirectToAction("Profile", "Profile");
	        }

			// Ensures the browser is not going to store a cached version of the Home page and if you hit back it will show it to the logged-in users 
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
			Response.Headers["Pragma"] = "no-cache";
			Response.Headers["Expires"] = "0";

			var model = new HomePageViewModel
			{
				RecentBigWins = await _homePageService.GetRecentBigWinsAsync()
			};

			return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
	}
}
