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

		public async Task<PaginatedEventList<EventBetStatsRow>> GetUpcomingEventBetStatsAsync(
			int page, int pageSize, CancellationToken ct = default)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 5, 100);

			var now = DateTime.UtcNow;

			var baseQ = _db.MatchEvents
				.AsNoTracking()
				.Where(e => e.MatchDate > now && !e.IsCancelled);

			var total = await baseQ.CountAsync(ct);

			var rows = await baseQ
				.OrderBy(e => e.MatchDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(e => new EventBetStatsRow
				{
					EventId = e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					LeagueName =
						!string.IsNullOrEmpty(e.HomeTeam.LeagueName) ? e.HomeTeam.LeagueName :
						!string.IsNullOrEmpty(e.AwayTeam.LeagueName) ? e.AwayTeam.LeagueName :
						"Unknown",
					KickoffUtc = e.MatchDate,

					TotalBets = e.Bets.Count(b => b.Status == BetStatus.Pending),
					HomeBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Home),
					DrawBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Draw),
					AwayBets = e.Bets.Count(b => b.Status == BetStatus.Pending && b.Pick == MatchPick.Away),
				})
				.ToListAsync(ct);

			foreach (var r in rows)
			{
				if (r.TotalBets <= 0) { r.HomePct = r.DrawPct = r.AwayPct = 0; continue; }

				r.HomePct = Math.Round(100m * r.HomeBets / r.TotalBets, 2);
				r.DrawPct = Math.Round(100m * r.DrawBets / r.TotalBets, 2);
				r.AwayPct = Math.Round(100m * r.AwayBets / r.TotalBets, 2);

				var drift = 100m - (r.HomePct + r.DrawPct + r.AwayPct);
				if (drift != 0)
				{
					if (r.HomePct >= r.DrawPct && r.HomePct >= r.AwayPct) r.HomePct += drift;
					else if (r.DrawPct >= r.AwayPct) r.DrawPct += drift;
					else r.AwayPct += drift;
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
