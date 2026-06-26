using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class OddsGeneratorService : IOddsGeneratorService
	{
		// ─────────────────────  Odds ranges (per agreed ground rules)───────────────────── 
		private const decimal FavoriteMin = 1.23m;
		private const decimal FavoriteMax = 1.35m;  // 50/50 home gets this
		private const decimal UnderdogMin = 1.36m;  // 50/50 away gets this
		private const decimal UnderdogMax = 1.95m;
		private const decimal DrawMin     = 1.40m;  // even match → draw most likely
		private const decimal DrawMax     = 1.85m;  // mismatch  → draw unlikely

		// ───────────────────── H2H blending ───────────────────── 
		// Each H2H match contributes 5% weight, capped at 30%
		// e.g. 2 H2H matches = 10%, 6+ H2H matches = 30%

		private const double H2HWeightPerMatch = 0.05;
		private const double H2HMaxWeight      = 0.30;

		private readonly MatchFixerDbContext _db;

		public OddsGeneratorService(MatchFixerDbContext db) => _db = db;

		public async Task<GeneratedOddsDto> GenerateAsync(
			Guid homeTeamId,
			Guid awayTeamId,
			CancellationToken ct = default)
		{
			// Pull all results from both data sources for each team
			var homeMatches = await GetAllMatchRecordsAsync(homeTeamId, ct);
			var awayMatches = await GetAllMatchRecordsAsync(awayTeamId, ct);

			// Time-weighted win rates (recent matches count more)
			var homeRate = CalculateWeightedWinRate(homeMatches, homeTeamId);
			var awayRate = CalculateWeightedWinRate(awayMatches, awayTeamId);

			// Blend with head-to-head history if enough data exists
			var h2hRecords = await GetH2HRecordsAsync(homeTeamId, awayTeamId, ct);
			if (h2hRecords.Count >= 2)
			{
				var h2hHomeRate = CalculateWeightedWinRate(h2hRecords, homeTeamId);
				var h2hAwayRate = CalculateWeightedWinRate(h2hRecords, awayTeamId);
				var h2hWeight   = Math.Min(H2HMaxWeight, h2hRecords.Count * H2HWeightPerMatch);

				homeRate = homeRate * (1 - h2hWeight) + h2hHomeRate * h2hWeight;
				awayRate = awayRate * (1 - h2hWeight) + h2hAwayRate * h2hWeight;
			}

			//  Map rates to odds ranges
			var diff      = homeRate - awayRate;     // > 0 home stronger, < 0 away stronger
			var magnitude = (decimal)Math.Abs(diff); // 0 = even, 1 = complete mismatch

			var drawOdds = Math.Round(DrawMin + magnitude * (DrawMax - DrawMin), 2);

			decimal homeOdds, awayOdds;
			if (diff >= 0)
			{
				// Home is favorite (home also gets the slight edge on exact 50/50)
				homeOdds = Math.Round(FavoriteMax - magnitude * (FavoriteMax - FavoriteMin), 2);
				awayOdds = Math.Round(UnderdogMin + magnitude * (UnderdogMax - UnderdogMin), 2);
			}
			else
			{
				// Away is favorite
				awayOdds = Math.Round(FavoriteMax - magnitude * (FavoriteMax - FavoriteMin), 2);
				homeOdds = Math.Round(UnderdogMin + magnitude * (UnderdogMax - UnderdogMin), 2);
			}

			return new GeneratedOddsDto
			{
				HomeOdds = homeOdds,
				DrawOdds = drawOdds,
				AwayOdds = awayOdds
			};
		}

		// ─────────────────────  Data fetching ───────────────────── 

		/// <summary>
		/// Combines API-imported historical results (MatchResults) with
		/// app-tracked results (LiveMatchResults → MatchEvent) for a single team.
		/// </summary>
		private async Task<List<MatchRecord>> GetAllMatchRecordsAsync(Guid teamId, CancellationToken ct)
		{
			var historical = await _db.MatchResults
				.AsNoTracking()
				.Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId)
				            && m.HomeScore != null && m.AwayScore != null)
				.Select(m => new MatchRecord(
					m.HomeTeamId,
					m.AwayTeamId,
					m.HomeScore!.Value,
					m.AwayScore!.Value,
					m.Date.UtcDateTime))
				.ToListAsync(ct);

			var live = await _db.LiveMatchResults
				.AsNoTracking()
				.Where(lr => lr.MatchEvent.HomeTeamId == teamId || lr.MatchEvent.AwayTeamId == teamId)
				.Select(lr => new MatchRecord(
					lr.MatchEvent.HomeTeamId,
					lr.MatchEvent.AwayTeamId,
					lr.HomeScore,
					lr.AwayScore,
					lr.RecordedAt))
				.ToListAsync(ct);

			return historical.Concat(live).ToList();
		}

		/// <summary>
		/// Returns all matches played between these two specific teams from both sources.
		/// </summary>
		private async Task<List<MatchRecord>> GetH2HRecordsAsync(Guid homeTeamId, Guid awayTeamId, CancellationToken ct)
		{
			var historical = await _db.MatchResults
				.AsNoTracking()
				.Where(m => ((m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId) ||
				             (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId))
				            && m.HomeScore != null && m.AwayScore != null)
				.Select(m => new MatchRecord(
					m.HomeTeamId,
					m.AwayTeamId,
					m.HomeScore!.Value,
					m.AwayScore!.Value,
					m.Date.UtcDateTime))
				.ToListAsync(ct);

			var live = await _db.LiveMatchResults
				.AsNoTracking()
				.Where(lr => (lr.MatchEvent.HomeTeamId == homeTeamId && lr.MatchEvent.AwayTeamId == awayTeamId) ||
				             (lr.MatchEvent.HomeTeamId == awayTeamId && lr.MatchEvent.AwayTeamId == homeTeamId))
				.Select(lr => new MatchRecord(
					lr.MatchEvent.HomeTeamId,
					lr.MatchEvent.AwayTeamId,
					lr.HomeScore,
					lr.AwayScore,
					lr.RecordedAt))
				.ToListAsync(ct);

			return historical.Concat(live).ToList();
		}

		// ───────────────────── Calculation helpers ───────────────────── 

		/// <summary>
		/// Weighted win rate where recency matters:
		/// last 90 days = 3×  |  last year = 2×  |  older = 1×
		/// Returns 0.5 (neutral) when no data exists.
		/// </summary>
		private static double CalculateWeightedWinRate(IEnumerable<MatchRecord> matches, Guid teamId)
		{
			double totalWeight = 0, weightedWins = 0;

			foreach (var m in matches)
			{
				var weight = TimeWeight(m.Date);
				totalWeight += weight;

				var won = (m.HomeTeamId == teamId && m.HomeScore > m.AwayScore) ||
				          (m.AwayTeamId == teamId && m.AwayScore > m.HomeScore);

				if (won) weightedWins += weight;
			}

			return totalWeight > 0 ? weightedWins / totalWeight : 0.5;
		}

		private static double TimeWeight(DateTime matchDate)
		{
			var daysSince = (DateTime.UtcNow - matchDate).TotalDays;
			return daysSince switch
			{
				< 90  => 3.0, // last 3 months — most relevant
				< 365 => 2.0, // last year     — still relevant
				_     => 1.0  // older          — still counts, lowest weight
			};
		}

		// ─────────────────────  Internal normalised record ───────────────────── 
		private record MatchRecord(Guid HomeTeamId, Guid AwayTeamId, int HomeScore, int AwayScore, DateTime Date);
	}
}
