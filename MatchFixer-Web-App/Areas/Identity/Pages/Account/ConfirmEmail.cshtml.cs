// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.Text;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MatchFixer_Web_App.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
		public async Task<IActionResult> OnGetAsync(string userId, string code, string? returnUrl = null)
		{
			if (userId == null || code == null)
			{
				return RedirectToPage("/Index");
			}

			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{userId}'.");
			}

			var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
			var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
			if (!result.Succeeded)
			{
				StatusMessage = "Error confirming your email.";
				return RedirectToPage("/Account/Login");
			}

			TempData["Success"] = "Your email has been successfully confirmed. You can now log in.";
			return RedirectToPage("/Account/Login");
		}
	}
}
