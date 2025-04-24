using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Models.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Views.Shared.Components.UserDropdown
{
	public class UserDropdownViewComponent : ViewComponent
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserDropdownViewComponent(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var user = await _userManager.GetUserAsync(HttpContext.User);

			if (user == null)
			{
				return View(null);
			}

			var model = new UserDropdownViewModel
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ProfileImageUrl = user.ProfilePicture?.ImageUrl ?? "/images/user.jpg"
			};

			return View(model);
		}
	}

}
