using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MatchFixer.Infrastructure.Entities;
using static MatchFixer.Common.ServiceConstants.UserCleanupConstants;


namespace MatchFixer.Core.Services
{
	public class UserCleanupService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<UserCleanupService> _logger;

		public UserCleanupService(IServiceProvider serviceProvider, ILogger<UserCleanupService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await CleanupUnconfirmedUsersAsync();
				await Task.Delay(CleanupInterval, stoppingToken);
			}
		}

		private async Task CleanupUnconfirmedUsersAsync()
		{
			_logger.LogInformation("------------------------------------Starting unconfirmed users cleanup task------------------------------------");

			using (var scope = _serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

				var expirationTime = DateTime.UtcNow.Subtract(AccountExpiration);

				var users = userManager.Users
					.Where(u => !u.EmailConfirmed && u.CreatedOn < expirationTime)
					.ToList();

				foreach (var user in users)
				{
					if (user.CreatedOn < expirationTime)
					{
						var result = await userManager.DeleteAsync(user);
						if (result.Succeeded)
						{
							_logger.LogInformation($"------------------------------------Deleted unconfirmed user: {user.Email}------------------------------------");
						}
						else
						{
							_logger.LogWarning($"Failed to delete user: {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
						}
					}
				}
			}

			_logger.LogInformation("------------------------------------Finished unconfirmed users cleanup task.------------------------------------");
		}

	}
}
