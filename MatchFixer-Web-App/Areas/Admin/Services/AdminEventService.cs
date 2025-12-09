using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminEventService: IAdminEventsService
	{
		private readonly MatchFixerDbContext _db;

		public AdminEventService(MatchFixerDbContext db)
		{
			_db = db;
		}

		public async Task<List<AdminEventOverviewDto>> GetFinishedEventsAsync(AdminEventHistoryFilters filters)
		{
			var query = _db.MatchEvents
				.Where(m => m.LiveResult != null || m.IsCancelled)
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Include(m => m.Bets)
					.ThenInclude(b => b.BetSlip)
					.ThenInclude(slip => slip.User)
				.Include(m => m.MatchEventLogs)
					.ThenInclude(l => l.ChangedBy)
				.AsQueryable();

			if (filters.FromDate.HasValue)
				query = query.Where(m => m.MatchDate >= filters.FromDate.Value);

			if (filters.ToDate.HasValue)
				query = query.Where(m => m.MatchDate <= filters.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(filters.League))
				query = query.Where(m => m.HomeTeam.LeagueName == filters.League);

			if (filters.TeamId.HasValue)
				query = query.Where(m => m.HomeTeamId == filters.TeamId || m.AwayTeamId == filters.TeamId);

			var events = await query.OrderByDescending(m => m.MatchDate).ToListAsync();

			var result = events.Select(e =>
			{
				var slips = e.Bets.Select(b => b.BetSlip).Distinct().ToList();

				decimal totalStake = slips.Sum(s => s.Amount);
				decimal totalPayout = slips.Where(s => s.IsSettled && s.WinAmount.HasValue)
										   .Sum(s => s.WinAmount.Value);

				return new AdminEventOverviewDto
				{
					EventId = e.Id,
					MatchName = $"{e.HomeTeam.Name} vs {e.AwayTeam.Name}",
					LeagueName = e.HomeTeam.LeagueName,
					MatchDate = e.MatchDate,
					HomeScore = e.LiveResult?.HomeScore,
					AwayScore = e.LiveResult?.AwayScore,
					IsCancelled = e.IsCancelled,

					TotalBets = e.Bets.Count,
					TotalStake = totalStake,
					TotalPayout = totalPayout,

					Bets = e.Bets.Select(b => new AdminBetSummaryDto
					{
						BetId = b.Id,
						Username = b.BetSlip.User.UserName,
						FullName = b.BetSlip.User.FullName,
						Pick = b.Pick.ToString(),
						Odds = b.Odds,
						Status = b.Status.ToString(),
						Stake = b.BetSlip.Amount,
						Payout = b.BetSlip.IsSettled ? b.BetSlip.WinAmount : null
					}).ToList(),

					Logs = e.MatchEventLogs.Select(l => new MatchEventLogDto
					{
						ChangedAt = l.ChangedAt,
						PropertyName = l.PropertyName,
						OldValue = l.OldValue,
						NewValue = l.NewValue,
						ChangedByFullName = l.ChangedBy.FullName,
						ChangedByUserName = l.ChangedBy.UserName
					}).OrderByDescending(l => l.ChangedAt).ToList()
				};
			}).ToList();

			return result;
		}





	}
}
