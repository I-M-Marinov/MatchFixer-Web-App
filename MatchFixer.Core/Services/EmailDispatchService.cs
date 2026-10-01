using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MatchFixer.Core.Services
{
	// Sends approved queued emails whose scheduled time (SendAfterUtc) is due.
	public class EmailDispatchService : BackgroundService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<EmailDispatchService> _logger;

		private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
		private const int BatchSize = 25;

		public EmailDispatchService(IServiceProvider services, ILogger<EmailDispatchService> logger)
		{
			_services = services;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _services.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<MatchFixerDbContext>();
					var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

					var now = DateTime.UtcNow;

					var due = await db.QueuedEmails
						.Where(x => x.Status == EmailQueueStatus.Approved
									&& (x.SendAfterUtc == null || x.SendAfterUtc <= now))
						.OrderBy(x => x.SendAfterUtc)
						.Take(BatchSize)
						.ToListAsync(stoppingToken);

					foreach (var email in due)
					{
						try
						{
							await emailSender.SendEmailAsync(email.Recipient, email.Subject, email.BodyHtml);
							email.Status = EmailQueueStatus.Sent;
							email.SentAtUtc = DateTime.UtcNow;
							email.AttemptCount++;
							email.LastError = null;
						}
						catch (Exception ex)
						{
							email.AttemptCount++;
							email.LastError = ex.Message.Length > 1000 ? ex.Message.Substring(0, 1000) : ex.Message;
							email.Status = EmailQueueStatus.Failed;
							_logger.LogError(ex, "Failed to dispatch queued email {Id} to {Recipient}", email.Id, email.Recipient);
						}
					}

					if (due.Count > 0)
						await db.SaveChangesAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "EmailDispatchService tick failed");
				}

				await Task.Delay(PollInterval, stoppingToken);
			}
		}
	}
}
