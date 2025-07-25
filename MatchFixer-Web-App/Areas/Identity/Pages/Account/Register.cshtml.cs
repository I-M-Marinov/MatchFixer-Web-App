﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using ISO3166;
using MatchFixer.Common.EmailTemplates;
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
using static MatchFixer.Common.EmailTemplates.EmailMessages;


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
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
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

                    var emailBody = EmailTemplates.WelcomeEmail(logoUrl, callbackUrl);

					await _emailSender.SendEmailAsync(Input.Email, EmailTemplates.SubjectPleaseConfirmEmail, emailBody);


					if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
	                    TempData["SuccessMessage"] = ConfirmationEmailSent; // Store success message
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
