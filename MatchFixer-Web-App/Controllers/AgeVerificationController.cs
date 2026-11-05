using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using MatchFixer.Common.GeneralConstants;

namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	public class AgeVerificationController : Controller
	{
		private const string OpenModalKey = "AgeVerifyOpen";
		private const string ModalErrorKey = "AgeVerifyError";

		private readonly IBettingEligibilityService _eligibility;

		public AgeVerificationController(IBettingEligibilityService eligibility) => _eligibility = eligibility;

		// Old /verify-age links open the modal instead
		[HttpGet("/verify-age")]
		public IActionResult Verify(string? returnUrl)
		{
			TempData[OpenModalKey] = true;
			return LocalRedirect(SafeReturnUrl(returnUrl));
		}

		[HttpPost("/verify-age")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Verify(DateTime? dateOfBirth, bool confirm, string? returnUrl, CancellationToken ct)
		{
			var back = SafeReturnUrl(returnUrl);

			if (dateOfBirth == null)
				return ReopenWithError(back, AgeVerificationConstants.MsgInvalidDob);

			if (!confirm)
				return ReopenWithError(back, "Please confirm that the date of birth you entered is correct.");

			var (ok, msg) = await _eligibility.SelfVerifyAgeAsync(CurrentUserId(), dateOfBirth.Value, ct);

			if (ok || msg == AgeVerificationConstants.MsgDobChangeSubmitted)
			{
				TempData[TempDataKeys.SuccessMessage] = msg;
				return LocalRedirect(back);
			}

			return ReopenWithError(back, msg);
		}

		private IActionResult ReopenWithError(string back, string message)
		{
			TempData[OpenModalKey] = true;
			TempData[ModalErrorKey] = message;
			return LocalRedirect(back);
		}

		private Guid CurrentUserId() =>
			Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

		private string SafeReturnUrl(string? returnUrl) =>
			!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
				? returnUrl
				: Url.Action("LiveEvents", "Event") ?? "/";
	}
}
