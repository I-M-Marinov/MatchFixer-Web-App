using MatchFixer.Infrastructure.Entities;
using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MatchFixer_Web_App.Models.Profile;
using MatchFixer.Core.ViewModels.Profile;

namespace MatchFixer_Web_App.Controllers
{
	public class ProfileController : Controller
	{
		private readonly IProfileService _profileService;


		public ProfileController(IProfileService profileService)
		{
			_profileService = profileService;
		}


		[HttpGet]
		public async Task<IActionResult> Profile()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

			var viewModel = await _profileService.GetProfileAsync(userId);
			if (viewModel == null)
			{
				return NotFound();
			}

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				// Check if there's a specific validation error for TimeZone
				if (ModelState.ContainsKey(nameof(model.TimeZone)) && ModelState[nameof(model.TimeZone)].Errors.Any())
				{
					// Retrieve the first error message for TimeZone and save it to TempData
					var timeZoneErrorMessage = ModelState[nameof(model.TimeZone)].Errors.FirstOrDefault()?.ErrorMessage;
					TempData["ErrorMessage"] = timeZoneErrorMessage ?? "Time Zone is missing or incorrect!";
				}
				else
				{
					var firstErrorMessage = ModelState.Values
						.SelectMany(v => v.Errors)
						.FirstOrDefault()?.ErrorMessage;

					// If we find any error message, save it to TempData
					TempData["ErrorMessage"] = firstErrorMessage ?? "Some other error occurred!";
				}

				return RedirectToAction("Profile"); // Redirect back to the Profile view
			}

			try
			{
				var (success, message) = await _profileService.UpdateProfileAsync(model);

				if (success)
				{
					TempData["SuccessMessage"] = message; // Store success message
				}
				else
				{
					TempData["ErrorMessage"] = message; // Store error or no-change
				}

				return RedirectToAction("Profile", "Profile");
			}
			catch (Exception ex)
			{
				// Handle the error and display it
				ModelState.AddModelError(string.Empty, $"Error updating profile: {ex.Message}");
				return View("Profile", model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			var (success, message) = await _profileService.ConfirmEmailAsync(userId, code);

			if (success)
			{
				TempData["SuccessMessage"] = message;
			}
			else
			{
				TempData["ErrorMessage"] = message;
			}
			
			return RedirectToAction("Profile", "Profile");
		}



	}
}
