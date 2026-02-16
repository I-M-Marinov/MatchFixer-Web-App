using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Wallet;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminWalletService
	{
		Task<bool> UserHasWalletAsync(Guid userId);
		Task<Wallet?> GetWalletAsync(Guid userId, bool includeTransactions = true);
		Task<AdminWalletViewModel?> GetWalletViewModelAsync(Guid userId, string timeZoneId);

		Task<(bool Success, string Message)> CreateWalletForUserAsync(Guid userId, string currency = "EUR");

		Task<(bool Success, string Message)> AdminDepositAsync(Guid userId, decimal amount, string? description = null, Guid? adminActorId = null);
		Task<(bool Success, string Message)> AdminWithdrawAsync(Guid userId, decimal amount, string? description = null, Guid? adminActorId = null);

		Task<(bool Success, string Message)> ClearTransactionHistoryAsync(Guid userId);

		Task<bool> ToggleWalletLockAsync(Guid userId, string? reason = null);

	}
}
