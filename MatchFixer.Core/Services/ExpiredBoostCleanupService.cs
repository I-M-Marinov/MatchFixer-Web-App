using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MatchFixer.Core.Services
{
	public sealed class ExpiredBoostCleanupService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<ExpiredBoostCleanupService> _logger;
		private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

		public ExpiredBoostCleanupService(
			IServiceScopeFactory scopeFactory,
			ILogger<ExpiredBoostCleanupService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("********************** ExpiredBoostCleanupService started. **********************");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await CleanupExpiredBoostsAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "********************** Error while cleaning expired odds boosts. **********************");
				}

				await Task.Delay(_interval, stoppingToken);
			}

			_logger.LogInformation("********************** ExpiredBoostCleanupService stopped. **********************");
		}

		private async Task CleanupExpiredBoostsAsync(CancellationToken ct)
		{
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<MatchFixerDbContext>();

			var now = DateTime.UtcNow;

			var expiredBoosts = await db.OddsBoosts
				.Where(b => b.IsActive && b.EndUtc <= now)
				.ToListAsync(ct);

			if (!expiredBoosts.Any())
				return;

			foreach (var boost in expiredBoosts)
			{
				boost.IsActive = false;
				boost.EndUtc = now;
			}

			await db.SaveChangesAsync(ct);

			_logger.LogInformation(
				"||||||||||||********************** Cleaned {Count} expired odds boost(s) at {UtcNow}. **********************||||||||||||",
				expiredBoosts.Count,
				now);
		}
	}
}