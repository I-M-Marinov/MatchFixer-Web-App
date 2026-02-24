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

			// Filters
			if (filters.FromDate.HasValue)
				query = query.Where(m => m.MatchDate >= filters.FromDate.Value);

			if (filters.ToDate.HasValue)
				query = query.Where(m => m.MatchDate <= filters.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(filters.League))
				query = query.Where(m => m.HomeTeam.LeagueName == filters.League);

			if (filters.TeamId.HasValue)
				query = query.Where(m => m.HomeTeamId == filters.TeamId || m.AwayTeamId == filters.TeamId);

			var events = await query
				.OrderByDescending(m => m.MatchDate)
				.ToListAsync();

			var result = events.Select(e =>
			{
				var slips = e.Bets.Select(b => b.BetSlip).Distinct().ToList();

				decimal totalStake = slips.Sum(s => s.Amount);

				var winningSlips = slips
					.Where(s =>
						s.IsSettled &&
						s.WinAmount.HasValue &&
						s.WinAmount.Value > 0 &&
						s.Bets
							.Where(b => b.MatchEventId == e.Id)
							.All(b => b.Status == BetStatus.Won || b.Status == BetStatus.Voided)
					)
					.ToList();

				decimal totalPayout = winningSlips.Sum(s => s.WinAmount!.Value);

				var matchResult = e.LiveResult;

				string leagueName = string.IsNullOrWhiteSpace(e.CompetitionName)
										? e.HomeTeam.LeagueName
										: "Rest of World";

				return new AdminEventOverviewDto
				{
					EventId = e.Id,
					MatchName = $"{e.HomeTeam.Name} vs {e.AwayTeam.Name}",
					LeagueName = leagueName,
					MatchDate = e.MatchDate,
					HomeScore = matchResult?.HomeScore,
					AwayScore = matchResult?.AwayScore,
					IsCancelled = e.IsCancelled,

					TotalBets = e.Bets.Count,
					TotalStake = totalStake,
					TotalPayout = totalPayout,

					Bets = e.Bets.Select(b =>
					{
						string computedStatus;

						if (e.IsCancelled)
						{
							computedStatus = BetStatus.Voided.ToString();
						}
						else if (!b.BetSlip.IsSettled)
						{
							computedStatus = BetStatus.Pending.ToString();
						}
						else if (matchResult != null)
						{
							bool won =
								(b.Pick == MatchPick.Home && matchResult.HomeScore > matchResult.AwayScore) ||
								(b.Pick == MatchPick.Away && matchResult.AwayScore > matchResult.HomeScore) ||
								(b.Pick == MatchPick.Draw && matchResult.HomeScore == matchResult.AwayScore);

							computedStatus = won ? BetStatus.Won.ToString() : BetStatus.Lost.ToString();
						}
						else
						{
							computedStatus = BetStatus.Pending.ToString();
						}

						return new AdminBetSummaryDto
						{
							BetId = b.Id,
							Username = b.BetSlip.User.UserName,
							FullName = b.BetSlip.User.FullName,
							Pick = b.Pick.ToString(),
							Odds = b.Odds,
							Status = computedStatus,
							Stake = b.BetSlip.Amount,
							Payout = computedStatus == BetStatus.Won.ToString() ? b.BetSlip.Amount * b.Odds : null
						};
					}).ToList(),

					Logs = e.MatchEventLogs
						.Select(l => new MatchEventLogDto
						{
							ChangedAt = l.ChangedAt,
							PropertyName = l.PropertyName,
							OldValue = l.OldValue,
							NewValue = l.NewValue,
							ChangedByFullName = l.ChangedBy.FullName,
							ChangedByUserName = l.ChangedBy.UserName
						})
						.OrderByDescending(l => l.ChangedAt)
						.ToList()
				};

			}).ToList();

			return result;
		}

		public async Task<List<AdminTeamBettingStatsDto>>
		GetTeamBettingStatsAsync(AdminEventHistoryFilters filters)
		{
			var betsQuery = _db.Bets
				.AsNoTracking()
				.Where(b =>
					!b.MatchEvent.IsPostponed &&
					(b.MatchEvent.LiveResult != null || b.MatchEvent.IsCancelled) &&
					(b.Status == BetStatus.Won || b.Status == BetStatus.Lost) && 
					b.Pick != MatchPick.Draw);  // ignore draws


			if (filters.FromDate.HasValue)
				betsQuery = betsQuery.Where(b =>
					b.MatchEvent.MatchDate >= filters.FromDate.Value);

			if (filters.ToDate.HasValue)
				betsQuery = betsQuery.Where(b =>
					b.MatchEvent.MatchDate <= filters.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(filters.League))
				betsQuery = betsQuery.Where(b =>
					b.MatchEvent.HomeTeam.LeagueName == filters.League ||
					b.MatchEvent.AwayTeam.LeagueName == filters.League);

			if (filters.TeamId.HasValue)
				betsQuery = betsQuery.Where(b =>
					b.MatchEvent.HomeTeamId == filters.TeamId ||
					b.MatchEvent.AwayTeamId == filters.TeamId);


			var raw = await betsQuery
				.Select(b => new
				{
					TeamId =
						b.Pick == MatchPick.Home
							? (Guid?)b.MatchEvent.HomeTeamId
							: (Guid?)b.MatchEvent.AwayTeamId,

					TeamName =
						b.Pick == MatchPick.Home
							? b.MatchEvent.HomeTeam.Name
							: b.MatchEvent.AwayTeam.Name,

					Logo =
						b.Pick == MatchPick.Home
							? b.MatchEvent.HomeTeam.LogoUrl
							: b.MatchEvent.AwayTeam.LogoUrl,

					BetCount = 1,

					Stake =
						b.BetSlip.Bets.Count > 0
							? b.BetSlip.Amount / b.BetSlip.Bets.Count
							: 0m,

					Payout =
						b.Status == BetStatus.Won && b.BetSlip.Bets.Count > 0
							? (b.BetSlip.Amount / b.BetSlip.Bets.Count) * b.Odds
							: 0m
				})
				.ToListAsync();


			var teamStats = raw
				.GroupBy(x => new { x.TeamId, x.TeamName, x.Logo })
				.Select(g => new AdminTeamBettingStatsDto
				{
					TeamId = g.Key.TeamId!.Value,
					TeamName = g.Key.TeamName!,
					LogoUrl = g.Key.Logo!,
					TotalBets = g.Sum(x => x.BetCount),
					TotalStake = Math.Round(g.Sum(x => x.Stake), 2),
					TotalPayout = Math.Round(g.Sum(x => x.Payout), 2)
				})
				.OrderByDescending(x => x.TotalBets)
				.ToList();

			return teamStats;
		}


	}
}
