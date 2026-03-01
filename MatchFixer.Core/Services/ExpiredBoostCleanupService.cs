using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static MatchFixer.Common.GeneralConstants.MatchEventLogConstants;

namespace MatchFixer.Core.Services
{
	public sealed class ExpiredBoostCleanupService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<ExpiredBoostCleanupService> _logger;
		private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

		public ExpiredBoostCleanupService(
			IServiceScopeFactory scopeFactory,
			ILogger<ExpiredBoostCleanupService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				"""
				===========================================
				Expired Boost Cleanup Service STARTED
				Interval: {Interval}
				Started At (UTC): {UtcNow}
				===========================================
				""",
				_interval,
				DateTime.UtcNow);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await CleanupExpiredBoostsAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex,
						"""
						===========================================
						ERROR during expired boost cleanup
						Time (UTC): {UtcNow}
						===========================================
						""",
						DateTime.UtcNow);
				}

				await Task.Delay(_interval, stoppingToken);
			}

			_logger.LogInformation(
				"""
				===========================================
				Expired Boost Cleanup Service STOPPED
				Stopped At (UTC): {UtcNow}
				===========================================
				""",
				DateTime.UtcNow);
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

				db.MatchEventLogs.Add(new MatchEventLog
				{
					MatchEventId = boost.MatchEventId,
					PropertyName = OddsBoostProperty,
					OldValue = null,
					NewValue = $"{BoostLifecycleAction.Expired.ToString().ToUpperInvariant()} | +{boost.BoostValue} | {boost.StartUtc:u} → {boost.EndUtc:u}",
					ChangedByUserId = boost.CreatedByUserId,
					ChangedAt = now
				});
			}

			await db.SaveChangesAsync(ct);

			_logger.LogInformation(
				"""
				-------------------------------------------
				Expired Boost Cleanup Completed
				Expired Boosts Processed: {Count}
				Processed At (UTC): {UtcNow}
				-------------------------------------------
				""",
				expiredBoosts.Count,
				now);
		}
	}
}