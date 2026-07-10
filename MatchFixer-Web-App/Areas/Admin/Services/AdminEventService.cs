using MatchFixer.Common.Enums;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminEventService: IAdminEventsService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IMemoryCache _cache;

		private static readonly MemoryCacheEntryOptions AggCacheOpts =
			new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

		// Cache key derived from filter fields that affect aggregate results (not Page/PageSize)
		private static string AggCacheKey(AdminEventHistoryFilters f) =>
			$"hist:agg:{f.FromDate:yyyyMMdd}:{f.ToDate:yyyyMMdd}:{f.League}:{f.TeamId}";

		private static string TeamStatsCacheKey(AdminEventHistoryFilters f) =>
			$"hist:teams:{f.FromDate:yyyyMMdd}:{f.ToDate:yyyyMMdd}:{f.League}:{f.TeamId}";

		private sealed record AggregateCache(
			decimal AllStake,
			decimal AllPayout,
			decimal AllLoss,
			decimal AllPositive,
			List<EventLeagueStat> LeagueStats
		);

		public AdminEventService(MatchFixerDbContext db, IMemoryCache cache)
		{
			_db    = db;
			_cache = cache;
		}

		public async Task<PaginatedEventList<AdminEventOverviewDto>> GetFinishedEventsAsync(AdminEventHistoryFilters filters)
		{
			int page     = Math.Max(1, filters.Page);
			int pageSize = Math.Clamp(filters.PageSize, 5, 50);
			int skip     = (page - 1) * pageSize;

			// Lean base query (no navigation includes) used for COUNT 
			var baseQuery = _db.MatchEvents
				.AsNoTracking()
				.Where(m => m.LiveResult != null || m.IsCancelled);

			if (filters.FromDate.HasValue)
				baseQuery = baseQuery.Where(m => m.MatchDate >= filters.FromDate.Value);
			if (filters.ToDate.HasValue)
				baseQuery = baseQuery.Where(m => m.MatchDate <= filters.ToDate.Value);
			if (!string.IsNullOrWhiteSpace(filters.League))
				baseQuery = baseQuery.Where(m => m.HomeTeam.LeagueName == filters.League);
			if (filters.TeamId.HasValue)
				baseQuery = baseQuery.Where(m => m.HomeTeamId == filters.TeamId || m.AwayTeamId == filters.TeamId);

			var totalCount = await baseQuery.CountAsync();

			// Paginated data query with full includes
			var events = await baseQuery
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Include(m => m.Bets)
					.ThenInclude(b => b.BetSlip)
						.ThenInclude(slip => slip.User)
				.Include(m => m.MatchEventLogs)
					.ThenInclude(l => l.ChangedBy)
				.OrderByDescending(m => m.MatchDate)
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync();

			// slipLegs only for the current page's bet slips 
			var pageSlipIds = events
				.SelectMany(e => e.Bets.Select(b => b.BetSlipId))
				.Distinct()
				.ToList();

			var slipLegs = await _db.Bets
				.Where(b => pageSlipIds.Contains(b.BetSlipId))
				.GroupBy(b => b.BetSlipId)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => x.Count);

			var result = events.Select(e =>
			{
				decimal totalStake = e.Bets.Sum(b =>
				{
					var legs = slipLegs.TryGetValue(b.BetSlipId, out var l) ? l : 1;
					return b.BetSlip.Amount / legs;
				});

				decimal totalPayout = e.Bets
					.Where(b =>
						b.BetSlip.IsSettled &&
						b.BetSlip.WinAmount.HasValue &&
						b.BetSlip.WinAmount > 0 &&
						b.BetSlip.Bets.All(x =>
							x.Status == BetStatus.Won || x.Status == BetStatus.Voided)
					)
					.Sum(b =>
					{
						var legs = slipLegs.TryGetValue(b.BetSlipId, out var l) ? l : 1;
						var stakePerLeg = b.BetSlip.Amount / legs;
						return stakePerLeg * b.Odds;
					});

				var matchResult = e.LiveResult;

				string leagueName = string.IsNullOrWhiteSpace(e.CompetitionName)
					? e.HomeTeam.LeagueName
					: e.CompetitionName == "FIFA World Cup"
						? "FIFA World Cup"
						: "Rest of World";

				return new AdminEventOverviewDto
				{
					EventId = e.Id,
					MatchName = $"{e.HomeTeam.Name} vs {e.AwayTeam.Name}",
					LeagueName = leagueName,
					MatchDate = e.MatchDate,
					HomeScore = matchResult?.HomeScore,
					AwayScore = matchResult?.AwayScore,
					HomeWonOnPenalties = matchResult?.HomeWonOnPenalties,
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
						else if (matchResult != null)
						{
							bool won = matchResult.HomeWonOnPenalties.HasValue
								? (b.Pick == MatchPick.Home &&  matchResult.HomeWonOnPenalties.Value) ||
								  (b.Pick == MatchPick.Away && !matchResult.HomeWonOnPenalties.Value)
								: (b.Pick == MatchPick.Home && matchResult.HomeScore > matchResult.AwayScore) ||
								  (b.Pick == MatchPick.Away && matchResult.AwayScore > matchResult.HomeScore) ||
								  (b.Pick == MatchPick.Draw && matchResult.HomeScore == matchResult.AwayScore);

							computedStatus = won ? BetStatus.Won.ToString() : BetStatus.Lost.ToString();
						}
						else
						{
							computedStatus = BetStatus.Pending.ToString();
						}

						BetSlipStatus slipStatus;

						if (b.BetSlip.IsSettled)
						{
							if (b.BetSlip.WinAmount.HasValue && b.BetSlip.WinAmount > 0)
								slipStatus = BetSlipStatus.Won;
							else
								slipStatus = BetSlipStatus.Lost;
						}
						else if (e.IsCancelled)
						{
							slipStatus = BetSlipStatus.Voided;
						}
						else
						{
							slipStatus = BetSlipStatus.Pending;
						}

						var legs = slipLegs.TryGetValue(b.BetSlipId, out var l) ? l : 1;
						var stakePerLeg = b.BetSlip.Amount / legs;


						return new AdminBetSummaryDto
						{
							BetId = b.Id,
							Username = b.BetSlip.User.UserName,
							FullName = b.BetSlip.User.FullName,
							Pick = b.Pick.ToString(),
							Odds = b.Odds,
							BetStatus = computedStatus,
							BetSlipStatus = slipStatus.ToString(),
							Stake = b.BetSlip.Amount,
							StakePerLeg = Math.Round(stakePerLeg, 2),
							Payout = slipStatus == BetSlipStatus.Won
								? stakePerLeg * b.Odds
								: null,
							Legs = legs
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

			// Aggregate totals across ALL filtered events — cached by filter key (not page)
			if (!_cache.TryGetValue(AggCacheKey(filters), out AggregateCache? agg))
			{
				var legCountSubquery = _db.Bets.AsNoTracking()
					.GroupBy(b => b.BetSlipId)
					.Select(g => new { BetSlipId = g.Key, Count = g.Count() });

				var allBetRows = await (
					from b  in _db.Bets.AsNoTracking()
					join m  in baseQuery                   on b.MatchEventId equals m.Id
					join bs in _db.BetSlips.AsNoTracking() on b.BetSlipId    equals bs.Id
					join lc in legCountSubquery            on b.BetSlipId    equals lc.BetSlipId
					join ht in _db.Teams.AsNoTracking()    on m.HomeTeamId   equals ht.Id
					select new
					{
						b.MatchEventId,
						b.Odds,
						bs.Amount,
						bs.IsSettled,
						bs.WinAmount,
						lc.Count,
						LeagueName = (m.CompetitionName == null || m.CompetitionName == "")
							? ht.LeagueName
							: m.CompetitionName == "FIFA World Cup"
								? "FIFA World Cup"
								: "Rest of World"
					}
				).ToListAsync();

				var perEventAggs = allBetRows
					.GroupBy(b => b.MatchEventId)
					.Select(g => new
					{
						LeagueName = g.First().LeagueName,
						Stake  = g.Sum(b => b.Amount / (decimal)b.Count),
						Payout = g.Where(b => b.IsSettled
										  && b.WinAmount.HasValue
										  && b.WinAmount > b.Amount)
								  .Sum(b => (b.Amount / (decimal)b.Count) * b.Odds)
					})
					.ToList();

				var leagueStats = perEventAggs
					.GroupBy(e => e.LeagueName)
					.Select(g => new EventLeagueStat
					{
						League      = g.Key ?? "Unknown",
						TotalProfit = g.Sum(e => e.Stake - e.Payout),
						EventCount  = g.Count()
					})
					.OrderByDescending(x => x.TotalProfit)
					.ToList();

				agg = new AggregateCache(
					AllStake    : perEventAggs.Sum(e => e.Stake),
					AllPayout   : perEventAggs.Sum(e => e.Payout),
					AllLoss     : perEventAggs.Where(e => e.Stake - e.Payout < 0).Sum(e => e.Stake - e.Payout),
					AllPositive : perEventAggs.Where(e => e.Stake - e.Payout > 0).Sum(e => e.Stake - e.Payout),
					LeagueStats : leagueStats
				);

				_cache.Set(AggCacheKey(filters), agg, AggCacheOpts);
			}

			return new PaginatedEventList<AdminEventOverviewDto>
			{
				Items                = result,
				Page                 = page,
				PageSize             = pageSize,
				TotalCount           = totalCount,
				AllEventsStake       = agg!.AllStake,
				AllEventsPayout      = agg.AllPayout,
				AllEventsLoss        = agg.AllLoss,
				AllEventsPositive    = agg.AllPositive,
				AllEventsLeagueStats = agg.LeagueStats
			};
		}

		public async Task<List<AdminTeamBettingStatsDto>>
		GetTeamBettingStatsAsync(AdminEventHistoryFilters filters)
		{
			if (_cache.TryGetValue(TeamStatsCacheKey(filters), out List<AdminTeamBettingStatsDto>? cached))
				return cached!;

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

			_cache.Set(TeamStatsCacheKey(filters), teamStats, AggCacheOpts);

			return teamStats;
		}


	}
}
