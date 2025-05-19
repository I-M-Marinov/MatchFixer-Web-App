// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using ISO3166;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;


namespace MatchFixer_Web_App.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUserService userService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _userService = userService;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Country is required.")]
            [Display(Name = "Country")]
            public string Country { get; set; }

            [Required]
            [Display(Name = "Timezone")]
            public string Timezone { get; set; }

			public List<SelectListItem> CountryOptions { get; set; } = new();
		}

        public List<SelectListItem> CountryOptions { get; set; }


		public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
	        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
	        Response.Headers["Pragma"] = "no-cache";
	        Response.Headers["Expires"] = "0";

	        if (User.Identity.IsAuthenticated) // if the user is already logged in redirect him to the profile page, not the register page 
			{
		        return RedirectToAction("Profile", "Profile");
	        }

			ReturnUrl = returnUrl;

            CountryOptions = Country.List
	            .OrderBy(c => c.Name)
	            .Select(c => new SelectListItem
	            {
		            Value = c.TwoLetterCode,
		            Text = c.Name
	            })
	            .ToList();

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			return Page();
		}

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
	        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
	        Response.Headers["Pragma"] = "no-cache";
	        Response.Headers["Expires"] = "0";

	        if (User.Identity.IsAuthenticated) // if the user is already logged in redirect him to the profile page, not the register page 
	        {
		        return RedirectToAction("Profile", "Profile");
	        }

			returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
	            var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.Country = Input.Country;
                user.TimeZone = Input.Timezone;
				user.FirstName = "New";
                user.LastName = "MatchFixer";
                user.ProfilePictureId = await _userService.GetOrCreateDefaultImageAsync();

				var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    _logger.LogInformation("Sending confirmation to: " + Input.Email);

                    var logoUrl = LogoUrl;

					var emailBody = $@"
					<!DOCTYPE html>
					<html>
					<head>
					    <meta charset='UTF-8'>
					    <title>Confirm Your Email</title>
					</head>
					<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
					    <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
					        <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
					            <img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
					        </div>
					        <div style='padding: 30px; text-align: center;'>
					            <h2 style='color: #333;'>🎉 Welcome to MatchFixer (Don’t worry, it’s legal... we checked)</h2>
					            <p style='color: #555; font-size: 16px;'>
					                Hey there, future match “fixer”! 😉
								<br />
									You're now part of the most suspiciously fun soccer betting platform on the planet.
								<br />
									(Just kidding. We keep it clean... mostly.)
								<br />
									Here at MatchFixer, you're not bribing refs — you're calling the shots with your football smarts.
								<br /> 
									From miracle goals to VAR-induced chaos, your bets could make legends (or memes).
								<br />
									So lace up those virtual boots.
								<br />
									It’s time to outwit, outbet, and maybe out-laugh the competition.
					                <br /><br />
					                Let's get you started on your journey!
					            </p>
								<p style='color: #bf2217; font-size: 12px;'>
										**Please note that this account would be active for 30 minutes and will be removed if not confirmed before time is up.
								</p>
					            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
					               style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; text-decoration: none; color: black; border-radius: 6px; font-weight: bold;'>
					                Confirm Your Email
					            </a>
					            <p style='margin-top: 30px; font-size: 13px; color: #888;'>
					                If you did not sign up for MatchFixer, please ignore this email.
					            </p>
								<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
					               All Rights Reserved. MatchFixer ® 2025
					            </p>
					        </div>
					    </div>
					</body>
					</html>
					";

					await _emailSender.SendEmailAsync(Input.Email, "MatchFixer - Please confirm your email", emailBody);


					if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
	                    TempData["SuccessMessage"] = "We have sent you a confirmation email. Please confirm your account."; // Store success message
						return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
					}
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await OnGetAsync(returnUrl);

			// If we got this far, something failed, redisplay form
			return Page();
		}

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
