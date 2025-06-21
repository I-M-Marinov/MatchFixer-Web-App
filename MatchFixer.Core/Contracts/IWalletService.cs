using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IWalletService
	{
		Task<bool> HasWalletAsync(Guid userId);
		Task<Wallet> GetWalletAsync(Guid userId);
		Task<Wallet> CreateWalletAsync(Guid userId);
	}
}
