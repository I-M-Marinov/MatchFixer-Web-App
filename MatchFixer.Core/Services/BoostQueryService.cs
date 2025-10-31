using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.DTO;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public sealed class BoostQueryService : IBoostQueryService
	{
		private readonly MatchFixerDbContext _db;

		public BoostQueryService(MatchFixerDbContext db)
		{
			_db = db;
		}

		public async Task<IReadOnlyList<ActiveBoost>> GetActiveBoostsAsync(IEnumerable<Guid> matchEventIds)
		{
			var ids = (matchEventIds ?? Enumerable.Empty<Guid>())
					  .Where(id => id != Guid.Empty)
					  .Distinct()
					  .ToList();

			if (ids.Count == 0)
			{
				return Array.Empty<ActiveBoost>();
			}

			var nowUtc = DateTime.UtcNow;

			var baseQ = _db.OddsBoosts
				.AsNoTracking()
				.Where(b => b.IsActive && b.StartUtc <= nowUtc && b.EndUtc >= nowUtc && ids.Contains(b.MatchEventId));


			var boostsForIds = await baseQ.ToListAsync();

			if (boostsForIds.Count == 0)
			{
				return Array.Empty<ActiveBoost>();
			}

			var latestActiveBoosts = boostsForIds
				.GroupBy(b => b.MatchEventId)
				.Select(g => g.OrderByDescending(b => b.StartUtc).First())
				.ToList();

			var matchIds = latestActiveBoosts.Select(b => b.MatchEventId).Distinct().ToList();

			var matchesQ = _db.MatchEvents
				.AsNoTracking()
				.Where(m => matchIds.Contains(m.Id))
				.Select(m => new
				{
					m.Id,
					m.HomeOdds,
					m.DrawOdds,
					m.AwayOdds,
					HomeTeamName = m.HomeTeam != null ? m.HomeTeam.Name : null,
					AwayTeamName = m.AwayTeam != null ? m.AwayTeam.Name : null
				});


			var matches = await matchesQ.ToDictionaryAsync(m => m.Id, m => m);

			var results = latestActiveBoosts.Select(b =>
			{
				matches.TryGetValue(b.MatchEventId, out var m);

				var baseHome = m?.HomeOdds ?? 0m;
				var baseDraw = m?.DrawOdds ?? 0m;
				var baseAway = m?.AwayOdds ?? 0m;

				return new ActiveBoost
				{
					MatchEventId = b.MatchEventId,
					BoostAmount = b.BoostValue,
					EffectiveHomeOdds = baseHome + b.BoostValue,
					EffectiveDrawOdds = baseDraw + b.BoostValue,
					EffectiveAwayOdds = baseAway + b.BoostValue,
					StartUtc = b.StartUtc,
					EndUtc = b.EndUtc,
					MaxStake = b.MaxStakePerBet,
					MaxUses = b.MaxUsesPerUser,
					Label = string.IsNullOrWhiteSpace(b.Note) ? "" : b.Note,
					HomeTeamName = m?.HomeTeamName,
					AwayTeamName = m?.AwayTeamName
				};
			}).ToList();

			return results;
		}

		public async Task<IReadOnlyList<ActiveBoost>> GetAllActiveBoostsAsync(int limit)
		{
			var nowUtc = DateTime.UtcNow;

			var baseQ = _db.OddsBoosts
				.AsNoTracking()
				.Where(b => b.IsActive && b.StartUtc <= nowUtc && b.EndUtc >= nowUtc);

			var active = await baseQ.ToListAsync();

			if (active.Count == 0) return Array.Empty<ActiveBoost>();

			var latestActive = active
				.GroupBy(b => b.MatchEventId)
				.Select(g => g.OrderByDescending(b => b.StartUtc).First())
				.OrderByDescending(b => b.StartUtc)
				.Take(limit)
				.ToList();

			var matchIds = latestActive.Select(b => b.MatchEventId).Distinct().ToList();

			var matchesQ = _db.MatchEvents
				.AsNoTracking()
				.Where(m => matchIds.Contains(m.Id))
				.Select(m => new
				{
					m.Id,
					m.HomeOdds,
					m.DrawOdds,
					m.AwayOdds,
					HomeTeamName = m.HomeTeam != null ? m.HomeTeam.Name : null,
					AwayTeamName = m.AwayTeam != null ? m.AwayTeam.Name : null
				});


			var matches = await matchesQ.ToDictionaryAsync(m => m.Id, m => m);

			var results = latestActive.Select(b =>
			{
				matches.TryGetValue(b.MatchEventId, out var m);

				var baseHome = m?.HomeOdds ?? 0m;
				var baseDraw = m?.DrawOdds ?? 0m;
				var baseAway = m?.AwayOdds ?? 0m;

				return new ActiveBoost
				{
					MatchEventId = b.MatchEventId,
					BoostAmount = b.BoostValue,
					EffectiveHomeOdds = baseHome + b.BoostValue,
					EffectiveDrawOdds = baseDraw + b.BoostValue,
					EffectiveAwayOdds = baseAway + b.BoostValue,
					StartUtc = b.StartUtc,
					EndUtc = b.EndUtc,
					MaxStake = b.MaxStakePerBet,
					MaxUses = b.MaxUsesPerUser,
					Label = string.IsNullOrWhiteSpace(b.Note) ? "BOOSTED" : b.Note,
					HomeTeamName = m?.HomeTeamName,
					AwayTeamName = m?.AwayTeamName
				};
			}).ToList();

			return results;
		}
	}
}
