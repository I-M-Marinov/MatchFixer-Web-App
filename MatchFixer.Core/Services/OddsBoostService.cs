using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Core.Contracts.IMatchEventNotifier;
using static MatchFixer.Common.GeneralConstants.OddsBoostConstants;

namespace MatchFixer.Core.Services
{
	public class OddsBoostService: IOddsBoostService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IMatchEventNotifier _notifier;


		public OddsBoostService(MatchFixerDbContext dbContext, IMatchEventNotifier notifier)
		{
			_dbContext = dbContext;
			_notifier = notifier;
		}

		public async Task<(decimal? home, decimal? draw, decimal? away, OddsBoost? boost)>
			GetEffectiveOddsAsync(Guid matchEventId, decimal? baseHome, decimal? baseDraw, decimal? baseAway, CancellationToken ct = default)
		{
			var now = DateTime.UtcNow;

			var boost = await _dbContext.OddsBoosts
				.AsNoTracking()
				.Where(b => b.MatchEventId == matchEventId && b.StartUtc <= now && b.EndUtc > now)
				.OrderByDescending(b => b.BoostValue)
				.ThenBy(b => b.StartUtc)
				.FirstOrDefaultAsync(ct);

			if (boost is null)
				return (baseHome, baseDraw, baseAway, null);

			decimal? home = Math.Round((decimal)baseHome + boost.BoostValue, 2, MidpointRounding.AwayFromZero);
			decimal? draw = Math.Round((decimal)baseDraw + boost.BoostValue, 2, MidpointRounding.AwayFromZero);
			decimal? away = Math.Round((decimal)baseAway + boost.BoostValue, 2, MidpointRounding.AwayFromZero);

			return (home, draw, away, boost);
		}

		public async Task<OddsBoost?> CreateOddsBoostAsync(
			Guid matchEventId,
			decimal boostValue,
			TimeSpan duration,
			Guid createdByUserId,
			DateTime? startUtc = null,
			decimal? maxStakePerBet = null,
			int? maxUsesPerUser = null,
			string? note = null,
			CancellationToken ct = default)
		{
			var start = (startUtc?.ToUniversalTime()) ?? DateTime.UtcNow;
			var end = start.Add(duration);

			// Validate input

			if (boostValue <= 0)
				throw new ArgumentException(BoostValueMustBePositive, nameof(boostValue));
			if (duration <= TimeSpan.Zero)
				throw new ArgumentException(DurationMustBePositive, nameof(duration));

			var match = await _dbContext.MatchEvents
				.AsNoTracking()
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.FirstOrDefaultAsync(m => m.Id == matchEventId, ct);

			if (match == null)
				throw new InvalidOperationException(MatchEventNotFound);

			// Disallow overlapping active boosts
			var overlapExists = await _dbContext.OddsBoosts
				.AnyAsync(b => b.MatchEventId == matchEventId &&
							   b.IsActive &&
							   b.EndUtc > start &&
							   b.StartUtc < end, ct);

			if (overlapExists)
				throw new InvalidOperationException(OverlappingBoostExists);

			var boost = new OddsBoost
			{
				Id = Guid.NewGuid(),
				MatchEventId = matchEventId,
				StartUtc = start,
				EndUtc = end,
				BoostValue = boostValue,
				MaxStakePerBet = maxStakePerBet,
				MaxUsesPerUser = maxUsesPerUser,
				IsActive = true,
				CreatedByUserId = createdByUserId,
				Note = note
			};

			await _dbContext.OddsBoosts.AddAsync(boost, ct);
			await _dbContext.SaveChangesAsync(ct);

			var effHome = Math.Round((match.HomeOdds ?? 0m) + boostValue, 2, MidpointRounding.AwayFromZero);
			var effDraw = Math.Round((match.DrawOdds ?? 0m) + boostValue, 2, MidpointRounding.AwayFromZero);
			var effAway = Math.Round((match.AwayOdds ?? 0m) + boostValue, 2, MidpointRounding.AwayFromZero);

			var now = DateTime.UtcNow;
			if (boost.IsActive && start <= now && end > now)
			{
				await _notifier.BroadcastBoostStartedAsync(new BoostRealtimeMessage
				{
					MatchEventId = matchEventId,
					EffectiveHomeOdds = effHome,
					EffectiveDrawOdds = effDraw,
					EffectiveAwayOdds = effAway,
					BoostValue = boostValue,
					StartUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc),
					EndUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc),
					MaxStake = maxStakePerBet,
					MaxUses = maxUsesPerUser,
					Label = string.IsNullOrWhiteSpace(note) ? "BOOST" : note,
					HomeName = match.HomeTeam.Name,
					AwayName = match.AwayTeam.Name,
					HomeTeamLogo = match.HomeTeam.LogoUrl,
					AwayTeamLogo = match.AwayTeam.LogoUrl
				}, ct);
			}


			await _notifier.NotifyBoostStartedAsync(
				matchEventId,
				effHome,
				effDraw,
				effAway,
				boostValue,
				startUtc: start,          
				boostEndUtc: end,           
				maxStake: maxStakePerBet ?? 0m,
				maxUses: maxUsesPerUser ?? 0
			);

			await LogBoostLifecycleAsync(
				matchEventId,
				BoostLifecycleAction.Started,
				createdByUserId,
				boostValue,
				start,
				end
			);

			await _dbContext.SaveChangesAsync(ct);

			return boost;
		}

		public async Task<OddsBoost?> StopOddsBoostAsync(Guid oddsBoostId, Guid stoppedByUserId, CancellationToken ct = default)
		{
			var now = DateTime.UtcNow;

			var boost = await _dbContext.OddsBoosts
				.Include(b => b.MatchEvent)
				.ThenInclude(m => m.HomeTeam)
				.Include(b => b.MatchEvent)
				.ThenInclude(m => m.AwayTeam)
				.FirstOrDefaultAsync(b => b.Id == oddsBoostId, ct);

			if (boost is null)
				throw new InvalidOperationException(BoostNotFound);

			// If already ended, ensure flag is consistent and return
			if (!boost.IsActive || boost.EndUtc <= now)
			{
				if (boost.IsActive)
				{
					boost.IsActive = false;
					boost.EndUtc = now;
					await _dbContext.SaveChangesAsync(ct);
				}

				return boost;
			}

			// Mark ended now
			boost.EndUtc = now;
			boost.IsActive = false;

			await _dbContext.SaveChangesAsync(ct);

			// Notify subscribers
			var match = boost.MatchEvent;
			var baseHome = match?.HomeOdds ?? 0m;
			var baseDraw = match?.DrawOdds ?? 0m;
			var baseAway = match?.AwayOdds ?? 0m;

			var effHome = Math.Round(baseHome, 2, MidpointRounding.AwayFromZero);
			var effDraw = Math.Round(baseDraw, 2, MidpointRounding.AwayFromZero);
			var effAway = Math.Round(baseAway, 2, MidpointRounding.AwayFromZero);

			await _notifier.NotifyMatchEventUpdatedAsync(
				boost.MatchEventId,
				baseHome,
				baseDraw,
				baseAway,
				effectiveHomeOdds: effHome,
				effectiveDrawOdds: effDraw,
				effectiveAwayOdds: effAway,
				activeBoostId: null
			);

			await LogBoostLifecycleAsync(
				boost.MatchEventId,
				BoostLifecycleAction.Stopped,
				stoppedByUserId,
				boost.BoostValue,
				boost.StartUtc,
				now
			);

			await _dbContext.SaveChangesAsync(ct);

			return boost;
		}

		private async Task LogBoostLifecycleAsync(
			Guid matchEventId,
			BoostLifecycleAction action,
			Guid userId,
			decimal boostValue,
			DateTime start,
			DateTime end)
		{
			await _dbContext.MatchEventLogs.AddAsync(new MatchEventLog
			{
				MatchEventId = matchEventId,
				PropertyName = MatchEventLogConstants.OddsBoostProperty,
				OldValue = null,
				NewValue = $"{action.ToString().ToUpperInvariant()} | +{boostValue} | {start:u} → {end:u}",
				ChangedByUserId = userId,
				ChangedAt = DateTime.UtcNow
			});
		}
	}
}
