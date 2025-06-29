// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MatchFixer.Infrastructure.Entities;

using static MatchFixer.Common.GeneralConstants.LoginConstants;

namespace MatchFixer_Web_App.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISessionService _sessionService;

        public LoginModel(
	        SignInManager<ApplicationUser> signInManager,
	        ILogger<LoginModel> logger,
	        UserManager<ApplicationUser> userManager,
	        ISessionService sessionService)
        {
	        _signInManager = signInManager;
	        _logger = logger;
	        _userManager = userManager;
            _sessionService = sessionService;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Required(ErrorMessage = EmailIsRequired)]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = PasswordIsRequired)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null, bool sessionExpired = false)
        {
	        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
	        Response.Headers["Pragma"] = "no-cache";
	        Response.Headers["Expires"] = "0";

	        if (User.Identity.IsAuthenticated) // if the user is already logged in redirect him to the profile page, not the login page 
	        {
		        return RedirectToAction("Profile", "Profile");
	        }

	        if (sessionExpired)
	        {
		        TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
	        }

			if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

			returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            return Page();
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
	        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
	        Response.Headers["Pragma"] = "no-cache";
	        Response.Headers["Expires"] = "0";

	        if (User.Identity.IsAuthenticated) // if the user is already logged in redirect him to the profile page, not the login page 
			{
		        return RedirectToAction("Profile", "Profile");
	        }

			returnUrl ??= Url.Content("~/");

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
	            var user = await _userManager.FindByEmailAsync(Input.Email);

				// Verify if the email the user is trying to get logged in actually exists and if not show him an error message
				if (user == null)
	            {
		            ModelState.AddModelError(string.Empty, AccountWithThatEmailExists);

					_logger.LogInformation($"An attempt to login with email: {Input.Email} was made.");
					return Page();
	            }

                // Verify if the user's email is confirmed and if not do not log him in, but instead show him an error message
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, EmailIsNotConfirmed);
                    return Page();
                }

                if (!user.IsActive && user.WasDeactivatedByAdmin)
                {
	                TempData["ErrorMessage"] = AccountDeactivatedByAdmin;
	                return Page();
                }

				if (!user.IsActive)
                {
	                user.IsActive = true;

	                var userUpdated = await _userManager.UpdateAsync(user);
	                if (!userUpdated.Succeeded)
	                {
		                TempData["ErrorMessage"] = FailedToReactivateAccount;
		                return RedirectToAction("Index", "Home");
	                }

					await _signInManager.RefreshSignInAsync(user); 

					TempData["SuccessMessage"] = $"Welcome back, {user.FullName}. Your account is active again !";
					return RedirectToAction("Profile", "Profile"); // redirect to the Profile View 
				}
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    if (!string.IsNullOrWhiteSpace(user.TimeZone))
                    {
	                    _sessionService.SetUserTimezone(user.TimeZone);
                    }

					return RedirectToAction("Profile", "Profile"); // redirect to the Profile View 
					//return LocalRedirect(returnUrl);
				}
				if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, LoginFailed);
                    return Page();
                }

			}

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
