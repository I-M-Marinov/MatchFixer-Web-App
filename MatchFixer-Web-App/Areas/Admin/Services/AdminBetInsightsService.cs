using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public sealed class AdminBetInsightsService : IAdminBetInsightsService
	{
		private readonly MatchFixerDbContext _db;
		public AdminBetInsightsService(MatchFixerDbContext db) => _db = db;

		public async Task<PaginatedEventList<EventBetStatsRow>>
	GetUpcomingEventBetStatsAsync(string? league, int page, int pageSize, CancellationToken ct = default)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 5, 100);

			var now = DateTime.UtcNow;

			var baseQ = _db.MatchEvents
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.AsNoTracking()
				.Where(e => e.MatchDate > now && !e.IsCancelled);

			// League filter 
			if (!string.IsNullOrWhiteSpace(league))
			{
				baseQ = baseQ.Where(e =>
					(e.HomeTeam.LeagueName != null && e.HomeTeam.LeagueName == league) ||
					(e.AwayTeam.LeagueName != null && e.AwayTeam.LeagueName == league));
			}

			var total = await baseQ.CountAsync(ct);

			var raw = await baseQ
				.OrderByDescending(e => e.Bets.Count)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(e => new
				{
					e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,

					LeagueName =
						!string.IsNullOrEmpty(e.HomeTeam.LeagueName) ? e.HomeTeam.LeagueName :
						!string.IsNullOrEmpty(e.AwayTeam.LeagueName) ? e.AwayTeam.LeagueName :
						"Unknown",

					KickoffUtc = e.MatchDate,

					e.HomeOdds,
					e.DrawOdds,
					e.AwayOdds,

					TotalBets = e.Bets.Count(b => b.Status == BetStatus.Pending),
					HomeBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Home),
					DrawBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Draw),
					AwayBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Away),
				})
				.ToListAsync(ct);

			var rows = new List<EventBetStatsRow>(raw.Count);

			foreach (var x in raw)
			{
				var r = new EventBetStatsRow
				{
					EventId = x.Id,
					HomeTeam = x.HomeTeam,
					AwayTeam = x.AwayTeam,
					HomeTeamLogoUrl = x.HomeTeamLogoUrl,
					AwayTeamLogoUrl = x.AwayTeamLogoUrl,
					LeagueName = x.LeagueName,
					KickoffUtc = x.KickoffUtc,
					TotalBets = x.TotalBets,
					HomeBets = x.HomeBets,
					DrawBets = x.DrawBets,
					AwayBets = x.AwayBets,
					TotalStake = 0m
				};

				if (r.TotalBets > 0)
				{
					r.HomePct = Math.Round(100m * r.HomeBets / r.TotalBets, 2);
					r.DrawPct = Math.Round(100m * r.DrawBets / r.TotalBets, 2);
					r.AwayPct = Math.Round(100m * r.AwayBets / r.TotalBets, 2);
				}
				else
				{
					decimal pH = x.HomeOdds is > 0 ? 1m / x.HomeOdds!.Value : 0m;
					decimal pD = x.DrawOdds is > 0 ? 1m / x.DrawOdds!.Value : 0m;
					decimal pA = x.AwayOdds is > 0 ? 1m / x.AwayOdds!.Value : 0m;

					var sum = pH + pD + pA;
					if (sum > 0)
					{
						r.HomePct = Math.Round(100m * pH / sum, 2);
						r.DrawPct = Math.Round(100m * pD / sum, 2);
						r.AwayPct = Math.Round(100m * pA / sum, 2);
					}
				}

				var drift = 100m - (r.HomePct + r.DrawPct + r.AwayPct);
				if (drift != 0)
				{
					if (r.HomePct >= r.DrawPct && r.HomePct >= r.AwayPct) r.HomePct += drift;
					else if (r.DrawPct >= r.AwayPct) r.DrawPct += drift;
					else r.AwayPct += drift;
				}

				rows.Add(r);
			}

			if (rows.Count > 0)
			{
				var eventIds = rows.Select(r => r.EventId).ToList();

				var perLeg = await _db.Bets
					.AsNoTracking()
					.Where(b => eventIds.Contains(b.MatchEventId) && b.Status == BetStatus.Pending)
					.GroupBy(b => new { b.BetSlipId, b.MatchEventId })
					.Select(g => new
					{
						g.Key.MatchEventId,
						PerLeg = g.Select(x => x.BetSlip.Amount).FirstOrDefault() / (decimal)g.Count()
					})
					.ToListAsync(ct);

				var totalStakeByEvent = perLeg
					.GroupBy(x => x.MatchEventId)
					.ToDictionary(g => g.Key, g => g.Sum(x => x.PerLeg));

				foreach (var r in rows)
				{
					if (totalStakeByEvent.TryGetValue(r.EventId, out var stake))
						r.TotalStake = stake;
				}
			}

			return new PaginatedEventList<EventBetStatsRow>
			{
				Items = rows,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};
		}

	}

}
