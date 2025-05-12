using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


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
			if (context.User.Identity.IsAuthenticated)
			{
				var user = await userManager.GetUserAsync(context.User);
				if (user != null)
				{
					var currentSessionUserId = context.Session.GetString("UserId");

					// reset the currently set currentSessionUserId if it is different from the user id of the user logged in now 
					if (currentSessionUserId == null || currentSessionUserId != user.Id.ToString())
					{
						context.Session.SetString("UserId", user.Id.ToString());
					}
				}
			}
			else
			{
				// Clear session on logout
				context.Session.Remove("UserId");
			}

			await _next(context);
		}

	}
}
