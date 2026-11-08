using MatchFixer.Common.Enums;
using MatchFixer.Common.FootballCompetitions;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Limits;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MatchFixer.Core.Services
{
	/// <summary>
	/// Betting limits from the admin Limits page:
	/// • market switches (site-wide) and the per-match "extra markets" switch,
	/// • max stake by league x number of legs (separate row for slips with extra markets),
	/// • max win per league. Slips mixing leagues use the average of their leagues' limits.
	/// </summary>
	public class BettingLimitsService : IBettingLimitsService
	{
		private const string CacheKey = "BettingLimits.Config";
		private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

		private readonly MatchFixerDbContext _db;
		private readonly IMemoryCache _cache;

		public BettingLimitsService(MatchFixerDbContext db, IMemoryCache cache)
		{
			_db = db;
			_cache = cache;
		}

		private sealed record CachedConfig(MarketSwitchesDto Markets, Dictionary<string, LeagueBetLimit> Leagues, LeagueBetLimit Default);

		private async Task<CachedConfig> LoadAsync(CancellationToken ct)
		{
			if (_cache.TryGetValue(CacheKey, out CachedConfig? cached) && cached != null)
				return cached;

			var settings = await _db.BettingLimitSettings.AsNoTracking()
				.FirstOrDefaultAsync(s => s.Id == BettingLimitSettings.SingletonId, ct);

			var rows = await _db.LeagueBetLimits.AsNoTracking().ToListAsync(ct);

			var markets = new MarketSwitchesDto
			{
				OverUnderEnabled = settings?.OverUnderEnabled ?? true,
				BothTeamsToScoreEnabled = settings?.BothTeamsToScoreEnabled ?? true,
				DoubleChanceEnabled = settings?.DoubleChanceEnabled ?? true
			};

			var defaultRow = rows.FirstOrDefault(r => r.IsDefault)
				?? new LeagueBetLimit { LeagueName = LeagueBetLimit.DefaultLeagueName };

			var leagues = rows
				.Where(r => !r.IsDefault)
				.GroupBy(r => r.LeagueName, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			var config = new CachedConfig(markets, leagues, defaultRow);
			_cache.Set(CacheKey, config, CacheDuration);
			return config;
		}

		private void InvalidateCache() => _cache.Remove(CacheKey);

		public async Task<MarketSwitchesDto> GetMarketSwitchesAsync(CancellationToken ct = default)
			=> (await LoadAsync(ct)).Markets;

		public async Task<bool> IsMarketOfferedAsync(Guid matchEventId, BettingMarket market, CancellationToken ct = default)
		{
			if (market == BettingMarket.MatchResult)
				return true;

			var config = await LoadAsync(ct);
			if (!config.Markets.IsEnabled(market))
				return false;

			var match = await _db.MatchEvents
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == matchEventId, ct);

			return match != null && match.IsMarketEnabled(market);
		}

		public async Task<SlipLimitsDto> GetSlipLimitsAsync(IReadOnlyCollection<SlipLeg> legs, decimal totalOdds, CancellationToken ct = default)
		{
			var matchIds = legs.Select(l => l.MatchId).Distinct().ToList();
			var result = new SlipLimitsDto
			{
				Legs = matchIds.Count,
				HasExtraMarkets = legs.Any(l => l.Market != BettingMarket.MatchResult)
			};

			if (matchIds.Count == 0)
				return result;

			var config = await LoadAsync(ct);

			// League of a match: its competition when it has one, otherwise the home team's league
			var leaguesByMatch = await _db.MatchEvents
				.AsNoTracking()
				.Where(m => matchIds.Contains(m.Id))
				.Select(m => new { m.Id, m.CompetitionName, HomeLeague = m.HomeTeam.LeagueName })
				.ToListAsync(ct);

			var stakeValues = new List<decimal>();
			var winValues = new List<decimal>();

			foreach (var m in leaguesByMatch)
			{
				var league = string.IsNullOrWhiteSpace(m.CompetitionName) ? m.HomeLeague : m.CompetitionName;
				config.Leagues.TryGetValue(league ?? string.Empty, out var row);

				// a blank league cell falls back to the Default row; a blank Default cell means no limit
				var stake = row?.StakeFor(result.Legs, result.HasExtraMarkets) ?? config.Default.StakeFor(result.Legs, result.HasExtraMarkets);
				var win = row?.MaxWin ?? config.Default.MaxWin;

				if (stake.HasValue) stakeValues.Add(stake.Value);
				if (win.HasValue) winValues.Add(win.Value);
			}

			// Mixed leagues: average of the legs' limits (legs without a limit don't lift it)
			if (stakeValues.Count > 0)
				result.MaxStake = RoundDown(stakeValues.Average());

			if (winValues.Count > 0)
			{
				result.MaxWin = RoundDown(winValues.Average());
				if (totalOdds > 0)
					result.MaxStakeForWin = RoundDown(result.MaxWin.Value / totalOdds);
			}

			result.EffectiveMaxStake = (result.MaxStake, result.MaxStakeForWin) switch
			{
				(decimal a, decimal b) => Math.Min(a, b),
				(decimal a, null) => a,
				(null, decimal b) => b,
				_ => null
			};

			// Odds-boost promos override every limit above: a slip with a capped promo pick
			// can only take the promo's max stake (lowest one when the slip has several promos).
			result.PromoMaxStake = await GetPromoMaxStakeAsync(
				legs.Where(l => l.Market == BettingMarket.MatchResult).Select(l => l.MatchId).Distinct().ToList(), ct);

			if (result.PromoMaxStake.HasValue)
				result.EffectiveMaxStake = result.PromoMaxStake;

			return result;
		}

		/// <summary>
		/// Lowest max stake among the boosts the slip's Match Result picks get. Picks the boost the same way as
		/// OddsBoostService.GetEffectiveOddsAsync (running now, highest boost first), so it matches the boosted odds.
		/// Boosts only apply to Match Result picks.
		/// </summary>
		private async Task<decimal?> GetPromoMaxStakeAsync(List<Guid> matchIds, CancellationToken ct)
		{
			if (matchIds.Count == 0)
				return null;

			var now = DateTime.UtcNow;

			var boosts = await _db.OddsBoosts
				.AsNoTracking()
				.Where(b => matchIds.Contains(b.MatchEventId) && b.StartUtc <= now && b.EndUtc > now)
				.Select(b => new { b.MatchEventId, b.BoostValue, b.StartUtc, b.MaxStakePerBet })
				.ToListAsync(ct);

			var caps = boosts
				.GroupBy(b => b.MatchEventId)
				.Select(g => g.OrderByDescending(b => b.BoostValue).ThenBy(b => b.StartUtc).First().MaxStakePerBet)
				.Where(cap => cap.HasValue)
				.Select(cap => cap!.Value)
				.ToList();

			return caps.Count > 0 ? caps.Min() : null;
		}

		private static decimal RoundDown(decimal value) => Math.Round(value, 2, MidpointRounding.ToZero);

		public async Task<BettingLimitsConfigDto> GetConfigAsync(CancellationToken ct = default)
		{
			var settings = await _db.BettingLimitSettings.AsNoTracking()
				.FirstOrDefaultAsync(s => s.Id == BettingLimitSettings.SingletonId, ct);

			var rows = await _db.LeagueBetLimits.AsNoTracking().ToListAsync(ct);

			var dto = new BettingLimitsConfigDto
			{
				Markets = new MarketSwitchesDto
				{
					OverUnderEnabled = settings?.OverUnderEnabled ?? true,
					BothTeamsToScoreEnabled = settings?.BothTeamsToScoreEnabled ?? true,
					DoubleChanceEnabled = settings?.DoubleChanceEnabled ?? true
				},
				MarketsUpdatedAtUtc = settings?.UpdatedAtUtc,
				MarketsUpdatedByUserId = settings?.UpdatedByUserId
			};

			var defaultRow = rows.FirstOrDefault(r => r.IsDefault);
			dto.Leagues.Add(defaultRow != null ? ToDto(defaultRow) : new LeagueLimitRowDto { LeagueName = LeagueBetLimit.DefaultLeagueName });
			dto.Leagues.AddRange(rows.Where(r => !r.IsDefault).OrderBy(r => r.LeagueName).Select(ToDto));

			return dto;
		}

		public async Task<string?> SaveConfigAsync(BettingLimitsConfigDto config, Guid adminUserId, CancellationToken ct = default)
		{
			var now = DateTime.UtcNow;

			// ── validate ──
			var incoming = (config.Leagues ?? new())
				.Where(r => !string.IsNullOrWhiteSpace(r.LeagueName))
				.Select(r => { r.LeagueName = r.LeagueName.Trim(); return r; })
				.ToList();

			var duplicate = incoming
				.GroupBy(r => r.LeagueName, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault(g => g.Count() > 1);
			if (duplicate != null)
				return $"\"{DisplayName(duplicate.Key)}\" is listed more than once.";

			foreach (var r in incoming)
			{
				if (r.LeagueName.Length > 100)
					return "League names can be at most 100 characters.";

				foreach (var (label, value) in Values(r))
				{
					if (value.HasValue && value.Value <= 0)
						return $"{DisplayName(r.LeagueName)}: {label} must be greater than 0 (leave it blank for no limit).";
					if (value.HasValue && value.Value > 10_000_000m)
						return $"{DisplayName(r.LeagueName)}: {label} is too large.";
				}
			}

			if (!incoming.Any(r => r.LeagueName == LeagueBetLimit.DefaultLeagueName))
				incoming.Insert(0, new LeagueLimitRowDto { LeagueName = LeagueBetLimit.DefaultLeagueName });

			// ── market switches ──
			var settings = await _db.BettingLimitSettings.FirstOrDefaultAsync(s => s.Id == BettingLimitSettings.SingletonId, ct);
			if (settings == null)
			{
				settings = new BettingLimitSettings { Id = BettingLimitSettings.SingletonId };
				_db.BettingLimitSettings.Add(settings);
			}

			var m = config.Markets ?? new MarketSwitchesDto();
			if (settings.OverUnderEnabled != m.OverUnderEnabled
				|| settings.BothTeamsToScoreEnabled != m.BothTeamsToScoreEnabled
				|| settings.DoubleChanceEnabled != m.DoubleChanceEnabled
				|| _db.Entry(settings).State == EntityState.Added)
			{
				settings.OverUnderEnabled = m.OverUnderEnabled;
				settings.BothTeamsToScoreEnabled = m.BothTeamsToScoreEnabled;
				settings.DoubleChanceEnabled = m.DoubleChanceEnabled;
				settings.UpdatedAtUtc = now;
				settings.UpdatedByUserId = adminUserId;
			}

			// ── league rows: update / add / remove ──
			var existing = await _db.LeagueBetLimits.ToListAsync(ct);

			foreach (var row in existing)
			{
				var match = incoming.FirstOrDefault(r => string.Equals(r.LeagueName, row.LeagueName, StringComparison.OrdinalIgnoreCase));
				if (match == null)
				{
					if (!row.IsDefault) _db.LeagueBetLimits.Remove(row);
					continue;
				}

				if (Apply(row, match))
				{
					row.UpdatedAtUtc = now;
					row.UpdatedByUserId = adminUserId;
				}
			}

			foreach (var r in incoming.Where(r => !existing.Any(e => string.Equals(e.LeagueName, r.LeagueName, StringComparison.OrdinalIgnoreCase))))
			{
				var row = new LeagueBetLimit
				{
					Id = Guid.NewGuid(),
					LeagueName = r.LeagueName,
					UpdatedAtUtc = now,
					UpdatedByUserId = adminUserId
				};
				Apply(row, r);
				_db.LeagueBetLimits.Add(row);
			}

			await _db.SaveChangesAsync(ct);
			InvalidateCache();
			return null;
		}

		public async Task<List<string>> GetAvailableLeaguesAsync(CancellationToken ct = default)
		{
			var teamLeagues = await _db.Teams
				.AsNoTracking()
				.Where(t => t.LeagueName != null && t.LeagueName != "")
				.Select(t => t.LeagueName)
				.Distinct()
				.ToListAsync(ct);

			var usedCompetitions = await _db.MatchEvents
				.AsNoTracking()
				.Where(m => m.CompetitionName != null && m.CompetitionName != "")
				.Select(m => m.CompetitionName!)
				.Distinct()
				.ToListAsync(ct);

			return teamLeagues
				.Concat(usedCompetitions)
				.Concat(CompetitionMappings.NameToId.Keys)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(n => n)
				.ToList();
		}

		public static string DisplayName(string leagueName)
			=> leagueName == LeagueBetLimit.DefaultLeagueName ? "Default" : leagueName;

		private static IEnumerable<(string Label, decimal? Value)> Values(LeagueLimitRowDto r)
		{
			yield return ("1-leg stake", r.MaxStake1);
			yield return ("2-leg stake", r.MaxStake2);
			yield return ("3-leg stake", r.MaxStake3);
			yield return ("4-leg stake", r.MaxStake4);
			yield return ("5+ leg stake", r.MaxStake5Plus);
			yield return ("1-leg extra-markets stake", r.ExtraMaxStake1);
			yield return ("2-leg extra-markets stake", r.ExtraMaxStake2);
			yield return ("3-leg extra-markets stake", r.ExtraMaxStake3);
			yield return ("4-leg extra-markets stake", r.ExtraMaxStake4);
			yield return ("5+ leg extra-markets stake", r.ExtraMaxStake5Plus);
			yield return ("max win", r.MaxWin);
		}

		/// <summary>Copies the values; returns true when anything changed.</summary>
		private static bool Apply(LeagueBetLimit row, LeagueLimitRowDto r)
		{
			static decimal? R(decimal? v) => v.HasValue ? Math.Round(v.Value, 2) : null;

			var before = (row.MaxStake1, row.MaxStake2, row.MaxStake3, row.MaxStake4, row.MaxStake5Plus,
				row.ExtraMaxStake1, row.ExtraMaxStake2, row.ExtraMaxStake3, row.ExtraMaxStake4, row.ExtraMaxStake5Plus, row.MaxWin);

			row.MaxStake1 = R(r.MaxStake1);
			row.MaxStake2 = R(r.MaxStake2);
			row.MaxStake3 = R(r.MaxStake3);
			row.MaxStake4 = R(r.MaxStake4);
			row.MaxStake5Plus = R(r.MaxStake5Plus);
			row.ExtraMaxStake1 = R(r.ExtraMaxStake1);
			row.ExtraMaxStake2 = R(r.ExtraMaxStake2);
			row.ExtraMaxStake3 = R(r.ExtraMaxStake3);
			row.ExtraMaxStake4 = R(r.ExtraMaxStake4);
			row.ExtraMaxStake5Plus = R(r.ExtraMaxStake5Plus);
			row.MaxWin = R(r.MaxWin);

			var after = (row.MaxStake1, row.MaxStake2, row.MaxStake3, row.MaxStake4, row.MaxStake5Plus,
				row.ExtraMaxStake1, row.ExtraMaxStake2, row.ExtraMaxStake3, row.ExtraMaxStake4, row.ExtraMaxStake5Plus, row.MaxWin);

			return !before.Equals(after);
		}

		private static LeagueLimitRowDto ToDto(LeagueBetLimit r) => new()
		{
			LeagueName = r.LeagueName,
			MaxStake1 = r.MaxStake1,
			MaxStake2 = r.MaxStake2,
			MaxStake3 = r.MaxStake3,
			MaxStake4 = r.MaxStake4,
			MaxStake5Plus = r.MaxStake5Plus,
			ExtraMaxStake1 = r.ExtraMaxStake1,
			ExtraMaxStake2 = r.ExtraMaxStake2,
			ExtraMaxStake3 = r.ExtraMaxStake3,
			ExtraMaxStake4 = r.ExtraMaxStake4,
			ExtraMaxStake5Plus = r.ExtraMaxStake5Plus,
			MaxWin = r.MaxWin,
			UpdatedAtUtc = r.UpdatedAtUtc,
			UpdatedByUserId = r.UpdatedByUserId
		};
	}
}
