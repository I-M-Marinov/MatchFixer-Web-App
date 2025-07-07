using MatchFixer.Common.EmailTemplates;
using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.Services;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;

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
		_logger.LogInformation("BirthdayEmailService started.");

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
						.Where(u => u.DateOfBirth.Month == today.Month && u.DateOfBirth.Day == today.Day)
						.ToListAsync(stoppingToken);

					var userIds = usersWithBirthdayToday.Select(u => u.Id).ToList();

					var alreadyAwardedUserIds = await dbContext.WalletTransactions
						.Where(t => t.CreatedAt.Date == today.Date &&
						            t.TransactionType == WalletTransactionType.BirthdayBonus &&
						            userIds.Contains(t.Wallet.UserId))
						.Select(t => t.Wallet.UserId)
						.ToListAsync(stoppingToken);

					var eligibleUsers = usersWithBirthdayToday
						.Where(u => !alreadyAwardedUserIds.Contains(u.Id))
						.ToList();

					if (eligibleUsers.Any())
					{
						const string subject = "🎂 Happy Birthday from MatchFixer!";
						var logoUrl = LogoUrl;

						foreach (var user in usersWithBirthdayToday)
						{
							string emailBody = EmailTemplates.BirthdayEmail(logoUrl, user.FullName);
							await emailService.SendEmailAsync(user.Email, subject, emailBody);
							_logger.LogInformation($"Sent birthday email to {user.Email}");

							await walletService.AwardBirthdayBonusAsync(user.Id);
							_logger.LogInformation($"Awarded €10 birthday bonus to {user.Email}");
						}
					}
					else
					{
						_logger.LogInformation("No birthdays today.");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while sending birthday emails.");
			}

			await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
		}

		_logger.LogInformation("BirthdayEmailService stopping.");
	}

}
