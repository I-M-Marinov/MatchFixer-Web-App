using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IWalletService
	{
		Task<bool> HasWalletAsync();
		Task<Wallet> GetWalletAsync();
		Task<WalletViewModel> GetWalletViewModelAsync(string timeZoneId);
		Task<Wallet> CreateWalletAsync();
		Task DepositAsync(decimal amount, string description = null);
		Task<bool> WithdrawAsync(decimal amount, string description = null);
		Task<(bool Success, string Message)> ClearTransactionHistoryAsync();
		Task<(bool Success, string Message)> DeductForBetAsync(Guid userId, decimal amount);
		Task AwardWinningsAsync(Guid userId, decimal amount, string matchDescription);
		Task<bool> AwardBirthdayBonusAsync(Guid userId);
		Task<bool> RefundBetAsync(Guid userId, decimal amount, Guid betSlipId);

	}
}
