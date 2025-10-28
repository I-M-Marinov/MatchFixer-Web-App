using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Core.Contracts.IMatchEventNotifier;

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
				.Where(b => b.MatchEventId == matchEventId && b.IsActive && b.StartUtc <= now && b.EndUtc > now)
				.OrderByDescending(b => b.BoostValue) // get the boost with the most value ( ex. if there is a 0.10 and 0.11 boosts it will get the 0.11 ) 
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
				throw new ArgumentException("Boost value must be greater than zero.", nameof(boostValue));
			if (duration <= TimeSpan.Zero)
				throw new ArgumentException("Duration must be greater than zero.", nameof(duration));

			var match = await _dbContext.MatchEvents
				.AsNoTracking()
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.FirstOrDefaultAsync(m => m.Id == matchEventId, ct);

			if (match == null)
				throw new InvalidOperationException("Match event not found.");

			// Disallow overlapping active boosts
			var overlapExists = await _dbContext.OddsBoosts
				.AnyAsync(b => b.MatchEventId == matchEventId &&
							   b.IsActive &&
							   b.EndUtc > start &&
							   b.StartUtc < end, ct);

			if (overlapExists)
				throw new InvalidOperationException("An active boost already overlaps with this timeframe.");

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
					StartUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc),
					EndUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc),
					MaxStake = maxStakePerBet,
					MaxUses = maxUsesPerUser,
					Label = string.IsNullOrWhiteSpace(note) ? "BOOST" : note,
					HomeName = match.HomeTeam.Name,
					AwayName = match.AwayTeam.Name
				}, ct);
			}


			await _notifier.NotifyBoostStartedAsync(
				matchEventId,
				effHome,
				effDraw,
				effAway,
				startUtc: start,          
				boostEndUtc: end,           
				maxStake: maxStakePerBet ?? 0m,
				maxUses: maxUsesPerUser ?? 0
			);

			return boost;
		}
	}
}
