using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Country = ISO3166.Country;

using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Profile;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Models.Image;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;
using static MatchFixer.Common.ServiceConstants.PasswordRequirements;
using static MatchFixer.Common.GeneralConstants.ProfileConstants;
using static MatchFixer.Common.GeneralConstants.AnonymizationConstants;


namespace MatchFixer.Core.Services
{
	public class ProfileService : IProfileService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IUrlHelper _urlHelper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ITimezoneService _timezoneService;
		private readonly MatchFixerDbContext _dbContext;
		private readonly IImageService _imageService;
		private readonly SignInManager<ApplicationUser> _signInManager;



		public ProfileService(
			UserManager<ApplicationUser> userManager,
			IEmailSender emailSender,
			IUrlHelperFactory urlHelperFactory,
			IHttpContextAccessor httpContextAccessor,
			ITimezoneService timezoneService,
			MatchFixerDbContext dbContext,
			IImageService imageService,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_emailSender = emailSender;


			var actionContext = new ActionContext
			{
				HttpContext = httpContextAccessor.HttpContext,
				RouteData = httpContextAccessor.HttpContext?.GetRouteData(),
				ActionDescriptor = new ActionDescriptor()
			};

			_httpContextAccessor = httpContextAccessor;

			_urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
			_timezoneService = timezoneService;
			_dbContext = dbContext;
			_imageService = imageService;
			_signInManager = signInManager;
		}

		public async Task<ApplicationUser> GetCurrentUser(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return null;
			}

			return user;
		}

		public async Task<ProfileViewModel> GetProfileAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return null;
			}

			// Load the user again, this time with the profile picture loaded eagerly
			user = await _dbContext.Users
				.Include(u => u.ProfilePicture)
				.FirstOrDefaultAsync(u => u.Id == user.Id);

			var countryOptions = Country.List
				.OrderBy(c => c.Name)
				.Select(c => new SelectListItem
				{
					Value = c.TwoLetterCode,
					Text = c.Name
				})
				.ToList();

			return new ProfileViewModel
			{
				Id = user.Id.ToString(),
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email!,
				DateOfBirth = user.DateOfBirth,
				Country = user.Country,
				TimeZone = user.TimeZone,
				CreatedOn = user.CreatedOn,
				MatchFixScore = user.MatchFixScore,
				UserRank = await GetUserRankAsync(user.Id.ToString()) ?? 0,
				ProfileImageUrl = user.ProfilePicture?.ImageUrl,
				CountryOptions = countryOptions
			};
		}

		public async Task<(bool Success, string Message)> UpdateProfileAsync(ProfileViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.Id);

			var changes = new List<string>();

			// Check if user changed the email and if yes update it + send a confirmation email to the new address
			if (user.Email != model.Email)
			{
				var scheme = _httpContextAccessor.HttpContext?.Request?.Scheme ?? "https";
				var (wasSuccess, returnMessage) = await UpdateEmailAsync(user.Id.ToString(), model.Email, scheme);
				changes.Add("Email");
			}

			// Check if user changed the date of birth and if yes update it
			if (user.DateOfBirth != model.DateOfBirth)
			{
				user.DateOfBirth = model.DateOfBirth;
				changes.Add("Date of Birth");
			}

			// Check if user changed the timezone and if yes update it 
			if (user.TimeZone != model.TimeZone)
			{
				var isValid = await IsValidTimezoneAsync(model.Country, model.TimeZone); // validate time zone
				if (!isValid)
				{
					return (false, TimeZoneMissingOrIncorrect);

				}

				user.TimeZone = model.TimeZone;
				changes.Add("Time Zone");
			}

			// Check if user changed the country and if yes update it ( update the timezone as well to be safe ) 
			if (user.Country != model.Country)
			{
				user.TimeZone = model.TimeZone;
				user.Country = model.Country;
				changes.Add("Country");
			}


			// If no changes were detected, return a message
			if (changes.Count == 0)
			{
				return (false, NoChangesMadeToProfile);
			}

			// Save the changes to the database
			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				return (false, FailedToUpdateProfile);
			}

			// If changes were made, return a message with the updated properties
			string message = ProfileUpdatedSuccessfully +
							 string.Join(", ", changes);
			return (true, message);
		}

		public async Task<(bool Success, string Message)> UpdateNamesAsync(ProfileViewModel model)
		{

			if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
			{
				return (false, FirstAndLastNameAreRequired);
			}

			var user = await _userManager.FindByIdAsync(model.Id);

			if (user == null)
			{
				return (false, UserNotFound);
			}

			var trimmedFirstName = model.FirstName?.Trim();
			var trimmedLastName = model.LastName?.Trim();

			var isFirstNameChanged = !string.Equals(user.FirstName, trimmedFirstName, StringComparison.Ordinal);
			var isLastNameChanged = !string.Equals(user.LastName, trimmedLastName, StringComparison.Ordinal);

			if (!isFirstNameChanged && !isLastNameChanged)
			{
				return (false, NoChangesMadeToProfile);
			}

			if (isFirstNameChanged)
			{
				user.FirstName = trimmedFirstName!;
			}

			if (isLastNameChanged)
			{
				user.LastName = trimmedLastName!;
			}

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return (false, $"Failed to update user: {errors}");
			}

			return (true, NameUpdatedSuccessfully);
		}

		public async Task<(bool Success, string ErrorMessage)> UpdateEmailAsync(string userId, string newEmail,
			string scheme)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return (false, UserNotFound);
			}

			if (user.Email != newEmail)
			{
				user.Email = newEmail;
				user.EmailConfirmed = false;
				user.UserName = newEmail;
				user.NormalizedUserName = _userManager.NormalizeName(newEmail);

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					return (false, FailedToUpdateUsersEmail);
				}

				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				var callbackUrl = _urlHelper.Action(
					"ConfirmEmail", "Profile",
					new { userId = user.Id, code = token },
					scheme
				);

				var logoUrl = LogoUrl;
				
				var emailBody = $@"
						<!DOCTYPE html>
						<html>
						<head>
						    <meta charset='UTF-8'>
						    <title>Confirm your email</title>
						</head>
						<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
						    <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
						        <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
						            <img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
						        </div>
						        <div style='padding: 30px; text-align: center;'>
						            <h2 style='color: #333;'>Please follow the link below to confirm your new Email Address</h2>
						            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
						               style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
						                Confirm Your Email
						            </a>
						            <p style='margin-top: 30px; font-size: 13px; color: #888;'>
						                If you did not request this email change, your account might be compromised.
						            </p>
									<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
						               All Rights Reserved. MatchFixer ® 2025
						            </p>
						        </div>
						    </div>
						</body>
						</html>
						";

				await _emailSender.SendEmailAsync(newEmail, "MatchFixer - Replace your email address", emailBody);

				return (true, EmailUpdatedSuccessfully);
			}

			return (false, NewAndCurrentEmailAreTheSame);
		}

		public async Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string code)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return (false, UserNotFound);
			}

			var result = await _userManager.ConfirmEmailAsync(user, code);
			if (result.Succeeded)
			{
				return (true, EmailConfirmedSuccessfully);
			}

			return (false, EmailConfirmationFailed);
		}

		public async Task<ImageResult> UploadProfilePictureAsync(string userId,
			ImageFileUploadModel imageFileUploadModel)
		{
			if (imageFileUploadModel.FormFile == null || imageFileUploadModel.FormFile.Length == 0)
			{
				return new ImageResult
				{
					IsSuccess = false,
					Message = NoFileUploaded
				};
			}

			var uploadResult = await _imageService.UploadImageAsync(imageFileUploadModel.FormFile);

			if (uploadResult.StatusCode == HttpStatusCode.OK)
			{
				// Retrieve current user with their profile picture

				var user = await _dbContext.Users
					.Include(u => u.ProfilePicture)
					.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));


				if (user.ProfilePictureId != DefaultImageId)
				{
					// Remove the profile picture that is to be changed the new one
					var deleteResult = await _imageService.DeleteImageAsync(user.ProfilePicture.PublicId);

					// Delete old picture entity if deletion from Cloudinary succeeded
					if (deleteResult.IsSuccess)
					{
						_dbContext.ProfilePictures.Remove(user.ProfilePicture);
					}
				}


				var newProfilePicture = new ProfilePicture
				{
					ImageUrl = uploadResult.Url.ToString(),
					PublicId = uploadResult.PublicId
				};

				await _dbContext.ProfilePictures.AddAsync(newProfilePicture);
				await _dbContext.SaveChangesAsync();

				user.ProfilePictureId = newProfilePicture.Id;

				await _userManager.UpdateAsync(user);

				return new ImageResult
				{
					IsSuccess = true,
					Message = ProfilePictureUploadedSuccessfully
				};
			}

			return new ImageResult
			{
				IsSuccess = false,
				Message = ProfilePictureUploadFailed
			};
		}

		public async Task<ImageResult> RemoveProfilePictureAsync(string userId)
		{

			var user = await _dbContext.Users
				.Include(u => u.ProfilePicture)
				.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

			if (user.ProfilePictureId == DefaultImageId)
			{
				return new ImageResult
				{
					IsSuccess = false,
					Message = DefaultPictureApplied
				};
			}

			if (string.IsNullOrEmpty(user.ProfilePicture.ImageUrl))
			{
				return new ImageResult
				{
					IsSuccess = false,
					Message = NoProfilePictureToRemove
				};
			}

			var deleteResult = await _imageService.DeleteImageAsync(user.ProfilePicture.PublicId);

			if (deleteResult.IsSuccess)
			{
				// Remove the profile picture URL from the user record

				_dbContext.ProfilePictures.Remove(user.ProfilePicture);

				user.ProfilePictureId = DefaultImageId; // assign the default user image 

				await _userManager.UpdateAsync(user);

				return new ImageResult
				{
					IsSuccess = true,
					Message = ProfilePictureSetToDefaultSuccessfully
				};
			}

			return new ImageResult
			{
				IsSuccess = false,
				Message = deleteResult.Message
			};
		}

		public async Task<int?> GetUserRankAsync(string userId)
		{
			var users = await _dbContext.Users
				.Where(u => u.IsActive == true)
				.OrderByDescending(u => u.MatchFixScore)
				.AsNoTracking()
				.ToListAsync();

			var rankedUsers = users
				.Select((user, index) => new { user.Id, Rank = index + 1 })
				.Take(10)
				.ToList();

			var userRank = rankedUsers.FirstOrDefault(u => u.Id == Guid.Parse(userId));
			return userRank?.Rank;
		}

		public async Task<bool> DeactivateUserAsync(string userId)
		{
			var user = await _dbContext.Users.FindAsync(Guid.Parse(userId));
			if (user != null)
			{
				user.IsActive = false;
				user.WasDeactivatedByAdmin = false;
				await _dbContext.SaveChangesAsync();
				return true;
			}

			return false;
		}

		public async Task<bool> AnonymizeUserAsync(string userId)
		{
			var user = await _dbContext.Users
				.Include(u => u.ProfilePicture)
				.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

			if (user != null)
			{
				user.FirstName = AnonymizedFirstName;
				user.LastName = AnonymizedLastName;
				user.Email = $"deleted-{Guid.NewGuid()}@ex-matchfixer.com";
				user.NormalizedEmail = user.Email.ToUpper();
				user.UserName = user.Email;
				user.NormalizedUserName = user.UserName.ToUpper();
				user.PhoneNumber = null;
				user.DateOfBirth = new DateTime(1900, 1, 1);
				user.Country = AnonymizedCountry;
				user.TimeZone = AnonymizedTimeZone;

				if (user.ProfilePictureId != DefaultImageId)
				{
					var deleteResult = await _imageService.DeleteImageAsync(user.ProfilePicture.PublicId);

					if (deleteResult.IsSuccess)
					{
						// Remove the picture from the Profile Pictures Table only if it is not the default profile picture

						if (user.ProfilePicture.Id != DefaultImageId)
						{
							_dbContext.ProfilePictures.Remove(user.ProfilePicture);
						}

					}
				}

				user.ProfilePictureId = DeletedUserImageId; // assign the deleted user image if the user's image is the default user image 
				user.IsActive = false;
				user.IsDeleted = true;
				user.WasDeactivatedByAdmin = false; // user decided to delete their account, it was not done by an administrator
				user.PasswordHash = null;

				await _dbContext.SaveChangesAsync();

				return true;

			}

			return false;
		}

		private async Task<bool> IsValidTimezoneAsync(string countryCode, string timezone)
		{
			return await _timezoneService.IsValidTimezoneAsync(countryCode, timezone);
		}

		public async Task ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword)
		{
			// check if the password complies to the application's password requirements 
			var validationResult = await ValidatePasswordAsync(newPassword);

			if (!validationResult.Succeeded)
			{
				var errors = validationResult.Errors.Select(e => e.Description);
				throw new InvalidOperationException($"Invalid password: {string.Join(", ", errors)}");
			}

			var user = await _userManager.GetUserAsync(userPrincipal);
			if (user == null)
			{
				throw new ArgumentException(UserNotFound);
			}

			var result = await _userManager.ChangePasswordAsync(
				user,
				currentPassword,
				newPassword);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Password change failed: {errors}");
			}

			await _signInManager.RefreshSignInAsync(user);
			await SendPasswordChangedEmailAsync(user);
		}

		private async Task SendPasswordChangedEmailAsync(ApplicationUser user)
		{
			var emailSubject = "MatchFixer - Your account password was changed";

			var logoUrl = LogoUrl;

			var emailBody = $@"
					<!DOCTYPE html>
					<html>
					<head>
					    <meta charset='UTF-8'>
					    <title>Reset Your Password</title>
					</head>
					<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
					    <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
					        <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
					            <img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
					        </div>
					        <div style='padding: 30px; text-align: center;'>
					            <h2 style='color: #333;'>Your password was changed on {_timezoneService.ConvertToUserTime(DateTime.UtcNow, user.TimeZone):dd MMM yyyy HH:mm}</h2>
					            
					            <p style='margin-top: 30px; font-size: 13px; color: #e32b17;'>
					                If you did not reset your password, please review your account and ensure it is not compromised.
					            </p>
								<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
					               All Rights Reserved. MatchFixer ® 2025
					            </p>
					        </div>
					    </div>
					</body>
					</html>
					";

			await _emailSender.SendEmailAsync(user.Email!, emailSubject, emailBody);
		}

		private async Task<IdentityResult> ValidatePasswordAsync(string password)
		{
			var options = new PasswordOptions
			{
				RequiredLength = MinLength,
				RequireDigit = RequireDigit,
				RequireLowercase = RequireLowercase,
				RequireUppercase = RequireUppercase,
				RequireNonAlphanumeric = RequireNonAlphanumeric
			};

			var validator = new PasswordValidator<ApplicationUser>();
			return await validator.ValidateAsync(_userManager, null, password);
		}


	}

}

