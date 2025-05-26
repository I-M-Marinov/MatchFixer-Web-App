using MatchFixer.Infrastructure.Models.Wikipedia;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface IWikipediaService
	{
		Task<WikiTeamInfo?> GetTeamInfoAsync(string teamName);
	}
}
