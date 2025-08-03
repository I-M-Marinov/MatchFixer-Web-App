using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MatchFixer.Core.ViewModels.Profile;
using System.ComponentModel.DataAnnotations;
using MatchFixer_Web_App.Areas.Identity.Pages.Account;
using MatchFixer.Infrastructure.Models.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer_Web_App.Controllers
{
	public class ProfileController : Controller
	{

		private readonly IProfileService _profileService;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<LogoutModel> _logger;
		private readonly ISessionService _sessionService;


		public ProfileController(IProfileService profileService, SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, ISessionService sessionService)
		{
			_profileService = profileService;
			_signInManager = signInManager;
			_logger = logger;
			_sessionService = sessionService;
		}

		[Authorize]
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

		[Authorize]
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

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateNames(ProfileViewModel model)
		{
			// Retrieve the current profile data using the service
			var currentProfile = await _profileService.GetProfileAsync(model.Id);

			model.Email = currentProfile.Email;
			model.DateOfBirth = currentProfile.DateOfBirth;
			model.Country = currentProfile.Country;
			model.TimeZone = currentProfile.TimeZone;
			model.ProfileImageUrl = currentProfile.ProfileImageUrl;
			model.CountryOptions = currentProfile.CountryOptions;
			model.CreatedOn = currentProfile.CreatedOn;

			ValidateModel(model);

			// Check if model state is valid after mapping the additional properties
			if (!ModelState.IsValid)
			{
				var allErrors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				TempData["ErrorMessage"] = allErrors.Any()
					? string.Join("<br/>", allErrors)
					: "An error occurred while updating your name.";

				return RedirectToAction("Profile");
			}

			// Call the service to update the user's name
			var result = await _profileService.UpdateNamesAsync(model);

			if (!result.Success)
			{
				TempData["ErrorMessage"] = result.Message;
				return RedirectToAction("Profile");
			}

			TempData["SuccessMessage"] = result.Message;
			return RedirectToAction("Profile");
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

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> UploadProfilePicture(ImageFileUploadModel imageFileUploadModel)
		{

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (ModelState.IsValid)
			{
				var result = await _profileService.UploadProfilePictureAsync(userId, imageFileUploadModel);

				if (result.IsSuccess)
				{
					TempData["SuccessMessage"] = result.Message;
				}
				else
				{
					TempData["ErrorMessage"] = result.Message;
				}
			}
			else
			{
				var firstErrorMessage = ModelState.Values
					.SelectMany(v => v.Errors)
					.FirstOrDefault()?.ErrorMessage;

				// If we find any error message, save it to TempData
				TempData["ErrorMessage"] = firstErrorMessage;
			}

			return RedirectToAction("Profile");
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> RemoveProfilePicture()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var result = await _profileService.RemoveProfilePictureAsync(userId);

			if (result.IsSuccess)
			{
				TempData["SuccessMessage"] = result.Message;
			}
			else
			{
				TempData["ErrorMessage"] = result.Message;
			}

			return RedirectToAction("Profile");
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetUserRank()
		{
			var userId = HttpContext.Session.GetString("UserId");

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "Session expired or user is not authenticated." });
			}

			var userRank = await _profileService.GetUserRankAsync(userId);

			if (userRank != null)
			{
				return Ok(new { rank = userRank });
			}

			return NotFound(new { message = "User not ranked in the top 3." });
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> DangerZone()
		{
			return View("DangerZone");
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteAccount()
		{
			var userId = HttpContext.Session.GetString("UserId");

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "Session expired or user is not authenticated." });
			}

			var result = await _profileService.AnonymizeUserAsync(userId);

			if (result)
			{
				LogoutUser();
				TempData["SuccessMessage"] = "Your account has been successfully deleted. Sorry to see you go !";
			}
			else
			{
				TempData["ErrorMessage"] = "There was an error deleting your account. Try again later !";
			}

			return RedirectToAction("Index", "Home");
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeactivateAccount()
		{
			var userId = HttpContext.Session.GetString("UserId");

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "Session expired or user is not authenticated." });
			}

			var result = await _profileService.DeactivateUserAsync(userId);

			if (result)
			{
				LogoutUser();
				TempData["SuccessMessage"] = "Your account has been deactivated successfully. Log in again to reactivate it !";
			}
			else
			{
				TempData["ErrorMessage"] = "There was an error deactivating your account. Try again later !";
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessage"] = $"Please correct the form errors.";
				return RedirectToAction("DangerZone");
			}

			try
			{
				await _profileService.ChangePasswordAsync(User, model.CurrentPassword, model.NewPassword);
				TempData["SuccessMessage"] = "Password was changed successfully!";
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Password change error");
				TempData["ErrorMessage"] = "An unexpected error occurred.";
			}

			return RedirectToAction("Profile"); // redirect to Profile when the password change is successful
		}


		private void ValidateModel(ProfileViewModel model)
		{
			ModelState.Clear();

			var validationResults = new List<ValidationResult>();
			var validationContext = new ValidationContext(model);

			if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
			{
				// If validation fails, add errors to ModelState
				foreach (var validationResult in validationResults)
				{
					ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
				}
			}
		}

		private void LogoutUser()
		{
			_signInManager.SignOutAsync();
			_sessionService.ClearSession();
			_logger.LogInformation("User logged out.");
		}

	}
}
