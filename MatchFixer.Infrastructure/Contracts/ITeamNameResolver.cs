using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface ITeamNameResolver
	{
		Task<Team?> ResolveTeamAsync(string inputName);
	}
}
