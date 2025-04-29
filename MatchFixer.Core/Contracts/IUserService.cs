
namespace MatchFixer.Core.Contracts
{
	public interface IUserService
	{
		Task<Guid> GetOrCreateDefaultImageAsync();
	}
}
