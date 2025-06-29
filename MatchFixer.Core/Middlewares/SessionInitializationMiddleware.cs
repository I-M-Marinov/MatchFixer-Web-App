using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using static MatchFixer.Common.GeneralConstants.SessionConstants;

namespace MatchFixer.Core.Middlewares
{
	public class SessionInitializationMiddleware
	{
		private readonly RequestDelegate _next;

		public SessionInitializationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
		{
			// Skip middleware for login, register, logout, static files, etc.
			var path = context.Request.Path.Value;

			if (string.IsNullOrEmpty(path) ||
			    (path.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase) && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) ||
			    path.StartsWith("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
			    path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
			{
				await _next(context);
				return;
			}

			if (context.User.Identity.IsAuthenticated)
			{
				var user = await userManager.GetUserAsync(context.User);
				if (user != null)
				{
					var currentSessionUserId = context.Session.GetString("UserId");

					// reset the currently set currentSessionUserId if it is different from the user id of the user logged in now 
					if (string.IsNullOrEmpty(currentSessionUserId) || currentSessionUserId != user.Id.ToString())
					{
						context.Session.SetString("UserId", user.Id.ToString());
					}

					if (string.IsNullOrEmpty(context.Session.GetString(UserTimezoneKey)))
					{
						// session expired
						await context.SignOutAsync(IdentityConstants.ApplicationScheme);
						context.Response.Redirect("/Identity/Account/Login?sessionExpired=true");
						return;
					}
				}
			}
			else
			{
				// clear on logout
				context.Session.Clear();
			}

			await _next(context);
		}


	}
}
