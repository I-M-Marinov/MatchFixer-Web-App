using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure.Models.FootballAPI;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchEventService
	{
		Task<List<LiveEventViewModel>> GetAllEventsAsync();
		Task<List<LiveEventViewModel>> GetLiveEventsAsync();
		Task AddEventAsync(MatchEventFormModel model);
		Task AddEventAsync(MatchEventFormModel model, int apiFixtureId);
		Task<string> GetTeamLogo(string name);
		Task<bool> UpdateCompetitionAsync(Guid eventId, string? competition);
		Task<Dictionary<string, List<SelectListItem>>> GetTeamsGroupedByLeagueAsync();
		Task<bool> EditMatchEventAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime? kickoffTime, bool noDraw = false);
		Task<bool> CancelMatchEventAsync(Guid matchEventId);
		Task<Dictionary<Guid, OddsDTO>> GetOddsForMatchesAsync(Guid[] matchIds);
		Task<int> BackfillMarketOddsAsync(CancellationToken ct = default);

		/// <summary>Extra-market prices generated from 1X2 odds, without saving anything (admin preview).</summary>
		Dictionary<string, decimal> PreviewMarketOdds(decimal? homeOdds, decimal? drawOdds, decimal? awayOdds, bool noDraw);

		/// <summary>Saves hand-set extra-market prices for a match and marks them as manual.</summary>
		Task<MarketOddsResultDto> UpdateMarketOddsAsync(Guid matchEventId, IDictionary<string, decimal> odds);

		/// <summary>Re-generates the extra-market prices from the match's 1X2 odds and clears the manual flag.</summary>
		Task<MarketOddsResultDto> RegenerateMarketOddsAsync(Guid matchEventId);

		/// <summary>
		/// Per-match switches: <paramref name="market"/> null = all extra markets of the match,
		/// otherwise one market (O/U, BTTS or Double Chance).
		/// </summary>
		Task<MarketOddsResultDto> SetExtraMarketsEnabledAsync(Guid matchEventId, bool enabled, MatchFixer.Common.Enums.BettingMarket? market = null);
		Task<List<ApiLeagueSelectViewModel>> GetApiLeaguesAsync();
		Task<bool> MatchExistsByApiFixtureAsync(int apiFixtureId);
		/// <summary>Returns null on success, or a human-readable skip reason when the match cannot be imported.</summary>
		Task<string?> AddEventFromUpcomingAsync(UpcomingMatchRowViewModel dto);
		Task<bool> PostponeMatchAsync(Guid matchEventId, Guid userId);
		Task<(bool Success, string Message)> RevertToUpcomingAsync(Guid matchEventId, DateTime? newKickoffLocal);
	}
}
