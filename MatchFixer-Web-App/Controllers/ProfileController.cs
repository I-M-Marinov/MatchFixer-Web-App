using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class ProfileController : Controller
	{
		public IActionResult Profile()
		{
			return View();
		}
	}
}
