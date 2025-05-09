using MatchFixer.Core.ViewModels.GameSessionState;

namespace MatchFixer.Core.Contracts
{
	public interface ISessionService
	{
		GameSessionState? GetSessionState();
		void SetSessionState(GameSessionState state);
		void InitializeSessionState(string userId);
		void ClearSession();
	}
}
