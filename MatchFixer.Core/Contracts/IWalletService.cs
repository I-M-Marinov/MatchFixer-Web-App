using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IWalletService
	{
		Task<bool> HasWalletAsync();
		Task<Wallet> GetWalletAsync(); 
		Task<WalletViewModel> GetWalletViewModelAsync();
		Task<Wallet> CreateWalletAsync();
		Task DepositAsync(decimal amount, string description = null);
		Task<bool> WithdrawAsync(decimal amount, string description = null);
		Task<(bool Success, string Message)> ClearTransactionHistoryAsync();
	}
}
