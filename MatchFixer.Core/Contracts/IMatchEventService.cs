using MatchFixer.Core.ViewModels.LiveEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchEventService
	{
		Task<List<LiveEventViewModel>> GetLiveEventsAsync();
		Task AddEventAsync(MatchEventFormModel model);
		Task<string> GetTeamLogo(string name);
	}
}
