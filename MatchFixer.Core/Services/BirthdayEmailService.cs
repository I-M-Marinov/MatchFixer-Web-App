using MatchFixer.Common.EmailTemplates;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;
using  static MatchFixer.Common.GeneralConstants.BirthdayEmailLogMessages;

public class BirthdayEmailService : BackgroundService
{
	private readonly IServiceProvider _services;
	private readonly ILogger<BirthdayEmailService> _logger;

	public BirthdayEmailService(IServiceProvider services, ILogger<BirthdayEmailService> logger)
	{
		_services = services;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation(ServiceStarted);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using (var scope = _services.CreateScope())
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<MatchFixerDbContext>();
					var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>();
					var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

					var today = DateTime.Today;

					var usersWithBirthdayToday = await dbContext.Users
						.Where(u => u.DateOfBirth.Month == today.Month &&
						            u.DateOfBirth.Day == today.Day)
						.ToListAsync(stoppingToken);

					if (!usersWithBirthdayToday.Any())
					{
						_logger.LogInformation(NoBirthdaysToday);
						await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
						continue;
					}


					foreach (var user in usersWithBirthdayToday)
					{
						string emailBody =
							EmailTemplates.BirthdayEmail(LogoUrl, user.FullName);

						await emailService.SendEmailAsync(
							user.Email,
							EmailTemplates.SubjectHappyBirthdayFromMatchFixer,
							emailBody);

						_logger.LogInformation(
							BirthdayEmailSent,
							user.Email
						);

						await walletService.AwardBirthdayBonusAsync(user.Id);

						_logger.LogInformation(
							BirthdayBonusAwarded,
							user.Email
						);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ProcessingError);
			}

			await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
		}

		_logger.LogInformation(ServiceStopping);
	}

}
