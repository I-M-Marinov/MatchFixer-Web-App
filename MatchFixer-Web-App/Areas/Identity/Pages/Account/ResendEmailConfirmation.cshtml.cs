// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MatchFixer_Web_App.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);


            var logoUrl = "https://res.cloudinary.com/doorb7d6i/image/upload/v1744732462/matchFixer-logo_kj93zj.png";

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
					            <h2 style='color: #333;'>Please follow the link below to confirm your MatchFixer Account</h2>
					            
					            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
					               style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
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


			await _emailSender.SendEmailAsync(
                Input.Email,
                "MatchFixer - Confirm your email",emailBody);

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}
