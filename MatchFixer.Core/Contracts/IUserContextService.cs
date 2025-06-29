using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IUserContextService
	{
		Guid GetUserId();
		Task<ApplicationUser?> GetCurrentUserAsync();
	}
}
