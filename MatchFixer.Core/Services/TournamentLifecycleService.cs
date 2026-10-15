using MatchFixer.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MatchFixer.Core.Services
{
	// Background worker that opens tournaments at their start time and settles
	// (ranks + pays out, or cancels + refunds) tournaments once they end.
	public sealed class TournamentLifecycleService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<TournamentLifecycleService> _logger;
		private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

		public TournamentLifecycleService(
			IServiceScopeFactory scopeFactory,
			ILogger<TournamentLifecycleService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				"""
				===========================================
				Tournament Lifecycle Service STARTED
				Interval: {Interval}
				Started At (UTC): {UtcNow}
				===========================================
				""",
				_interval,
				DateTime.UtcNow);

			// Let startup seeding finish before the first tick.
			try
			{
				await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
			}
			catch (TaskCanceledException)
			{
				return;
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();
					var tournaments = scope.ServiceProvider.GetRequiredService<ITournamentService>();
					await tournaments.RunLifecycleAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex,
						"""
						===========================================
						ERROR during tournament lifecycle tick
						Time (UTC): {UtcNow}
						===========================================
						""",
						DateTime.UtcNow);
				}

				try
				{
					await Task.Delay(_interval, stoppingToken);
				}
				catch (TaskCanceledException)
				{
					break;
				}
			}

			_logger.LogInformation(
				"""
				===========================================
				Tournament Lifecycle Service STOPPED
				Stopped At (UTC): {UtcNow}
				===========================================
				""",
				DateTime.UtcNow);
		}
	}
}
