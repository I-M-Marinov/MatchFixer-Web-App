using MatchFixer.Common.Enums;
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
		Task<(bool Success, string Message)> DeductForBetAsync(Guid userId, decimal amount, Guid betslipId);
		Task AwardWinningsAsync(Guid userId, decimal amount, string betslipDescription);
		Task<bool> AwardBirthdayBonusAsync(Guid userId);
		Task<bool> RefundBetAsync(Guid userId, decimal amount, Guid betSlipId);
		Task<bool> HasTransactionForSlipAsync(Guid userId, WalletTransactionType type, Guid betSlipId);

		// Tournaments (real-money entry fees and prize payouts)
		Task<(bool Success, string Message)> ChargeTournamentEntryAsync(Guid userId, decimal amount, Guid tournamentId);
		Task AwardTournamentPrizeAsync(Guid userId, decimal amount, Guid tournamentId, int rank);
		Task RefundTournamentEntryAsync(Guid userId, decimal amount, Guid tournamentId);
		Task<bool> HasTournamentTransactionAsync(Guid userId, WalletTransactionType type, Guid tournamentId);

	}
}
