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
			if (context.User.Identity.IsAuthenticated && context.Session.GetString("UserId") == null)
			{
				var user = await userManager.GetUserAsync(context.User);
				if (user != null)
				{
					context.Session.SetString("UserId", user.Id.ToString());
				}
			}

			await _next(context);
		}
	}
}
