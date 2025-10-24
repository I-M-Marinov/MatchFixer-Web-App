using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Core.Contracts.IBoostQueryService;

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
				return Array.Empty<ActiveBoost>();

			var now = DateTime.UtcNow;

			var latestActiveBoosts =
				await _db.OddsBoosts
					.AsNoTracking()
					.Where(b =>
						b.IsActive &&
						b.StartUtc <= now &&
						b.EndUtc >= now &&
						ids.Contains(b.MatchEventId))
					.GroupBy(b => b.MatchEventId)
					.Select(g => g.OrderByDescending(b => b.StartUtc).First())
					.ToListAsync();

			if (latestActiveBoosts.Count == 0)
				return Array.Empty<ActiveBoost>();

			var matchIds = latestActiveBoosts.Select(b => b.MatchEventId).Distinct().ToList();

			// Pull the base odds + team names 
			var matches = await _db.MatchEvents
				.AsNoTracking()
				.Where(m => matchIds.Contains(m.Id))
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Select(m => new
				{
					m.Id,
					m.HomeOdds,
					m.DrawOdds,
					m.AwayOdds,
					HomeTeamName = m.HomeTeam.Name,
					AwayTeamName = m.AwayTeam.Name
				})
				.ToDictionaryAsync(m => m.Id);

			// Project to ActiveBoost with effective odds = base + BoostValue
			var results = latestActiveBoosts
				.Where(b => matches.ContainsKey(b.MatchEventId))
				.Select(b =>
				{
					var m = matches[b.MatchEventId];

					// Business rule: additive bump (e.g., +0.25)
					var effHome = (m.HomeOdds ?? 0m) + b.BoostValue;
					var effDraw = (m.DrawOdds ?? 0m) + b.BoostValue;
					var effAway = (m.AwayOdds ?? 0m) + b.BoostValue;

					return new ActiveBoost
					{
						MatchEventId = b.MatchEventId,
						EffectiveHomeOdds = effHome,
						EffectiveDrawOdds = effDraw,
						EffectiveAwayOdds = effAway,
						StartUtc = b.StartUtc,
						EndUtc = b.EndUtc,
						MaxStake = b.MaxStakePerBet,   
						MaxUses = b.MaxUsesPerUser,  
						Label = string.IsNullOrWhiteSpace(b.Note) ? "BOOST" : b.Note,
						HomeTeamName = m.HomeTeamName,
						AwayTeamName = m.AwayTeamName
					};
				})
				.ToList();

			return results;
		}
	}
}
