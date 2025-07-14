using MatchFixer.Core.ViewModels.LiveEvents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchEventService
	{
		Task<List<LiveEventViewModel>> GetAllEventsAsync();
		Task<List<LiveEventViewModel>> GetLiveEventsAsync();
		Task AddEventAsync(MatchEventFormModel model);
		Task<string> GetTeamLogo(string name);
		Task<Dictionary<string, List<SelectListItem>>> GetTeamsGroupedByLeagueAsync();
		Task<bool> EditMatchEventAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime kickoffTime);
		Task<bool> CancelMatchEventAsync(Guid matchEventId);
	}
}
