using MatchFixer.Core.ViewModels.DTO;

namespace MatchFixer.Core.Contracts
{
	public interface IBoostQueryService
	{
		Task<IReadOnlyList<ActiveBoost>> GetActiveBoostsAsync(IEnumerable<Guid> matchEventIds);
		Task<IReadOnlyList<ActiveBoost>> GetAllActiveBoostsAsync(int limit);
	}
}
