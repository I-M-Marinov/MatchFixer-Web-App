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
			_logger.LogInformation(
				"""
				===========================================
				User Cleanup Background Service STARTED
				Cleanup Interval: {Interval}
				Account Expiration: {Expiration}
				Started At (UTC): {UtcNow}
				===========================================
				""",
				CleanupInterval,
				AccountExpiration,
				DateTime.UtcNow);

			while (!stoppingToken.IsCancellationRequested)
			{
				await CleanupUnconfirmedUsersAsync();
				await Task.Delay(CleanupInterval, stoppingToken);
			}
		}

		private async Task CleanupUnconfirmedUsersAsync()
		{
			_logger.LogInformation(
				"""
				-------------------------------------------
				Starting Unconfirmed User Cleanup
				Execution Time (UTC): {UtcNow}
				-------------------------------------------
				""",
				DateTime.UtcNow);

			using (var scope = _serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

				var expirationTime = DateTime.UtcNow.Subtract(AccountExpiration);

				var users = userManager.Users
					.Where(u => !u.EmailConfirmed && u.CreatedOn < expirationTime)
					.ToList();

				_logger.LogInformation(
					"""
					Users eligible for cleanup: {UserCount}
					Expiration Threshold (UTC): {ExpirationThreshold}
					""",
					users.Count,
					expirationTime);

				foreach (var user in users)
				{
					if (user.CreatedOn < expirationTime)
					{
						var result = await userManager.DeleteAsync(user);

						if (result.Succeeded)
						{
							_logger.LogInformation(
								"""
								Deleted Unconfirmed User
								Email: {Email}
								UserId: {UserId}
								Created On: {CreatedOn}
								""",
								user.Email,
								user.Id,
								user.CreatedOn);
						}
						else
						{
							_logger.LogWarning(
								"""
								Failed to delete unconfirmed user
								Email: {Email}
								UserId: {UserId}
								Errors: {Errors}
								""",
								user.Email,
								user.Id,
								string.Join(", ", result.Errors.Select(e => e.Description)));
						}
					}

					_logger.LogInformation(
						"""
						-------------------------------------------
						Finished Unconfirmed User Cleanup
						Deleted Users: {DeletedCount}
						Finished At (UTC): {UtcNow}
						-------------------------------------------
						""",
						users.Count,
						DateTime.UtcNow);
				}
			}
		}
	}
}
