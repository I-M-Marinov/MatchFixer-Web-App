using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
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
			GetUpcomingEventBetStatsAsync(
				string? league,
				string? competition,
				int page,
				int pageSize,
				EventSort sort,
				bool desc,
				CancellationToken ct = default)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 5, 100);

			var now = DateTime.UtcNow;

			var allEventsQ = _db.MatchEvents
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.AsNoTracking()
				.Where(e =>
					!e.IsCancelled &&
					!e.IsPostponed &&
					e.Status != MatchStatus.FullTime
				);
			var leagueCounts = await allEventsQ
				.Where(e => e.CompetitionName == null)
				.Select(e =>
					!string.IsNullOrEmpty(e.HomeTeam.LeagueName)
						? e.HomeTeam.LeagueName
						: !string.IsNullOrEmpty(e.AwayTeam.LeagueName)
							? e.AwayTeam.LeagueName
							: "Unknown")
				.GroupBy(x => x)
				.Select(g => new { League = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.League, x => x.Count, ct);


			var baseQ = allEventsQ;

			if (!string.IsNullOrWhiteSpace(competition))
			{
				// COMPETITION MODE
				baseQ = baseQ.Where(e => e.CompetitionName == competition);
			}
			else if (!string.IsNullOrWhiteSpace(league))
			{
				// LEAGUE MODE 
				baseQ = baseQ.Where(e =>
					e.CompetitionName == null &&
					e.HomeTeam.LeagueName == league &&
					e.AwayTeam.LeagueName == league);
			}

			// Competition Counts 
			var competitionCounts = await allEventsQ
				.Where(e => !string.IsNullOrWhiteSpace(e.CompetitionName))
				.Select(e => e.CompetitionName!.Trim())
				.GroupBy(name => name)
				.Select(g => new
				{
					Competition = g.Key,
					Count = g.Count()
				})
				.ToDictionaryAsync(x => x.Competition, x => x.Count, ct);



			var total = await baseQ.CountAsync(ct);


			var raw = await baseQ
				.Select(e => new
				{
					e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					IsLive =
						e.MatchDate <= now &&
						e.Status != MatchStatus.FullTime &&
						!e.IsCancelled &&
						!e.IsPostponed,
					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
					HomeTeamLocalLogoUrl = e.HomeTeam.LocalLogoUrl,
					AwayTeamLocalLogoUrl = e.AwayTeam.LocalLogoUrl,

					LeagueName =
						!string.IsNullOrEmpty(e.HomeTeam.LeagueName) ? e.HomeTeam.LeagueName :
						!string.IsNullOrEmpty(e.AwayTeam.LeagueName) ? e.AwayTeam.LeagueName :
						"Unknown",
					CompetitionName = string.IsNullOrWhiteSpace(e.CompetitionName)
						? null
						: e.CompetitionName,
					KickoffUtc = e.MatchDate,

					e.HomeOdds,
					e.DrawOdds,
					e.AwayOdds,

					TotalBets = e.Bets.Count(b => b.BetSlip.TournamentEntryId == null && b.Market == BettingMarket.MatchResult),
					HomeBets = e.Bets.Count(b => b.BetSlip.TournamentEntryId == null && b.Market == BettingMarket.MatchResult && b.Pick == MatchPick.Home),
					DrawBets = e.Bets.Count(b => b.BetSlip.TournamentEntryId == null && b.Market == BettingMarket.MatchResult && b.Pick == MatchPick.Draw),
					AwayBets = e.Bets.Count(b => b.BetSlip.TournamentEntryId == null && b.Market == BettingMarket.MatchResult && b.Pick == MatchPick.Away),
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
					IsLive = x.IsLive,
					HomeTeamLogoUrl = x.HomeTeamLogoUrl,
					AwayTeamLogoUrl = x.AwayTeamLogoUrl,
					HomeTeamLocalLogoUrl = x.HomeTeamLocalLogoUrl,
					AwayTeamLocalLogoUrl = x.AwayTeamLocalLogoUrl,
					LeagueName = x.LeagueName,
					KickoffUtc = x.KickoffUtc,
					TotalBets = x.TotalBets,
					CompetitionName = x.CompetitionName,
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

				// Every non-tournament bet leg on these events, with its per-leg stake:
				// the slip amount split evenly across all legs in the slip
				// (same convention used across the rest of the admin area).
				var legs = await _db.Bets
					.AsNoTracking()
					.Where(b => eventIds.Contains(b.MatchEventId) && b.BetSlip.TournamentEntryId == null)
					.Select(b => new
					{
						b.MatchEventId,
						b.Market,
						b.Selection,
						PerLeg = b.BetSlip.Amount / b.BetSlip.Bets.Count
					})
					.ToListAsync(ct);

				var legsByEvent = legs
					.GroupBy(x => x.MatchEventId)
					.ToDictionary(g => g.Key, g => g.ToList());

				foreach (var r in rows)
				{
					if (!legsByEvent.TryGetValue(r.EventId, out var evLegs))
						continue;

					// Main row stays pure 1X2: Total Stake counts Match Result legs only.
					r.TotalStake = evLegs
						.Where(x => x.Market == BettingMarket.MatchResult)
						.Sum(x => x.PerLeg);

					// Accordion breakdown: the extra markets, bets + stake per selection.
					r.Markets = evLegs
						.Where(x => x.Market != BettingMarket.MatchResult)
						.GroupBy(x => x.Market)
						.OrderBy(g => (int)g.Key)
						.Select(mg => new EventMarketBreakdown
						{
							Market = mg.Key,
							MarketName = MarketDisplayName(mg.Key),
							TotalBets = mg.Count(),
							TotalStake = mg.Sum(x => x.PerLeg),
							Selections = mg
								.GroupBy(x => x.Selection ?? "")
								.OrderBy(sg => SelectionOrder(mg.Key, sg.Key))
								.Select(sg => new EventMarketSelectionStat
								{
									Selection = sg.Key,
									Label = SelectionLabel(mg.Key, sg.Key),
									Bets = sg.Count(),
									Stake = sg.Sum(x => x.PerLeg),
									Pct = mg.Count() > 0
										? Math.Round(100m * sg.Count() / mg.Count(), 2)
										: 0m
								})
								.ToList()
						})
						.ToList();
				}
			}

			IEnumerable<EventBetStatsRow> sorted = rows;

			sorted = sort switch
			{
				EventSort.TotalStake => desc
					? sorted.OrderByDescending(r => r.TotalStake)
					: sorted.OrderBy(r => r.TotalStake),

				EventSort.TotalBets => desc
					? sorted.OrderByDescending(r => r.TotalBets)
					: sorted.OrderBy(r => r.TotalBets),

				EventSort.Kickoff => desc
					? sorted.OrderByDescending(r => r.KickoffUtc)
					: sorted.OrderBy(r => r.KickoffUtc),

				_ => sorted
			};

			var paged = sorted
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PaginatedEventList<EventBetStatsRow>
			{
				Items = paged,
				Page = page,
				PageSize = pageSize,
				TotalCount = total,
				LeagueEventCounts = leagueCounts,
				CompetitionCounts = competitionCounts 

			};
		}

		private static string MarketDisplayName(BettingMarket m) => m switch
		{
			BettingMarket.OverUnderGoals => $"Over / Under {MarketSelections.DefaultGoalsLine}",
			BettingMarket.BothTeamsToScore => "Both Teams To Score",
			BettingMarket.DoubleChance => "Double Chance",
			_ => m.ToString()
		};

		private static string SelectionLabel(BettingMarket m, string sel) => m switch
		{
			BettingMarket.OverUnderGoals =>
				sel == MarketSelections.Over ? $"Over {MarketSelections.DefaultGoalsLine}" :
				sel == MarketSelections.Under ? $"Under {MarketSelections.DefaultGoalsLine}" : sel,
			BettingMarket.BothTeamsToScore =>
				sel == MarketSelections.Yes ? "Yes" :
				sel == MarketSelections.No ? "No" : sel,
			BettingMarket.DoubleChance => sel switch
			{
				MarketSelections.HomeOrDraw => "Home or Draw (1X)",
				MarketSelections.HomeOrAway => "Home or Away (12)",
				MarketSelections.DrawOrAway => "Draw or Away (X2)",
				_ => sel
			},
			_ => sel
		};

		private static int SelectionOrder(BettingMarket m, string sel)
		{
			var order = m switch
			{
				BettingMarket.OverUnderGoals => MarketSelections.OverUnder,
				BettingMarket.BothTeamsToScore => MarketSelections.BothTeamsToScore,
				BettingMarket.DoubleChance => MarketSelections.DoubleChance,
				_ => Array.Empty<string>()
			};
			var idx = Array.IndexOf(order, sel);
			return idx < 0 ? 999 : idx;
		}

	}

}
