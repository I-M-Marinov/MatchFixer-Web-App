using MatchFixer.Common.Enums;
using MatchFixer.Core.DTOs.Limits;

namespace MatchFixer.Core.Contracts
{
	public interface IBettingLimitsService
	{
		/// <summary>Site-wide market switches (cached).</summary>
		Task<MarketSwitchesDto> GetMarketSwitchesAsync(CancellationToken ct = default);

		/// <summary>True when the market is switched on site-wide and for the match.</summary>
		Task<bool> IsMarketOfferedAsync(Guid matchEventId, BettingMarket market, CancellationToken ct = default);

		/// <summary>
		/// Stake / win limits for a slip: the league x legs table (extra-market row when the slip has an
		/// extra-market pick), averaged across the leagues of its matches, plus the max-win cap at <paramref name="totalOdds"/>.
		/// </summary>
		Task<SlipLimitsDto> GetSlipLimitsAsync(IReadOnlyCollection<SlipLeg> legs, decimal totalOdds, CancellationToken ct = default);

		/// <summary>Everything the admin Limits page shows.</summary>
		Task<BettingLimitsConfigDto> GetConfigAsync(CancellationToken ct = default);

		/// <summary>Saves the admin Limits page. Returns an error message, or null on success.</summary>
		Task<string?> SaveConfigAsync(BettingLimitsConfigDto config, Guid adminUserId, CancellationToken ct = default);

		/// <summary>League / competition names to choose from when adding a limits row.</summary>
		Task<List<string>> GetAvailableLeaguesAsync(CancellationToken ct = default);
	}
}
