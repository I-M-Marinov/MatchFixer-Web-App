using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.ViewModels.GameSessionState;

namespace MatchFixer.Core.Contracts
{
	public interface ISessionService
	{
		GameSessionState? GetSessionState();
		void SetSessionState(GameSessionState state);
		void InitializeSessionState(string userId);
		void ClearSession();
		void SetBetSlipState(BetSlipState state);
		void ClearBetSlip();
		BetSlipState? GetBetSlipState();
		void AddBetToSlip(BetSlipItem item);
		void SetUserTimezone(string timezoneId);
		string GetUserTimezone();
	}
}
