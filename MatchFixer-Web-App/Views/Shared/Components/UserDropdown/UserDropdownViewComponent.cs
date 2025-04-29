using MatchFixer.Infrastructure.Entities;
using MatchFixer.Core.ViewModels.Profile;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Views.Shared.Components.UserDropdown
{
	public class UserDropdownViewComponent : ViewComponent
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly MatchFixerDbContext _dbContext;

		public UserDropdownViewComponent(UserManager<ApplicationUser> userManager, MatchFixerDbContext dbContext)
		{
			_userManager = userManager;
			_dbContext = dbContext;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var user = await _userManager.GetUserAsync(HttpContext.User);

			if (user == null)
			{
				return View(null);
			}

			// Load the user again, this time with the profile picture loaded eagerly
			user = await _dbContext.Users
				.Include(u => u.ProfilePicture)
				.FirstOrDefaultAsync(u => u.Id == user.Id);

			var model = new UserDropdownViewModel
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ProfileImageUrl = user.ProfilePicture?.ImageUrl
			};

			return View(model);
		}
	}

}
