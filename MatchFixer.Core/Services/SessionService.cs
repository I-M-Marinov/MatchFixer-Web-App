using MatchFixer.Core.Contracts;
using MatchFixer.Core.Extensions;
using MatchFixer.Core.ViewModels.GameSessionState;
using Microsoft.AspNetCore.Http;

using static MatchFixer.Common.GeneralConstants.SessionConstants;

namespace MatchFixer.Core.Services
{
	public class SessionService : ISessionService
	{
		private const string SessionKey = GameSessionName;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public SessionService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public GameSessionState? GetSessionState()
		{
			return _httpContextAccessor.HttpContext?.Session.GetObject<GameSessionState>(SessionKey);
		}

		public void SetSessionState(GameSessionState state)
		{
			_httpContextAccessor.HttpContext?.Session.SetObject(SessionKey, state);
		}

		public void InitializeSessionState(string userId)
		{
			var state = new GameSessionState
			{
				UserId = userId,
				Score = 0,
				QuestionNumber = 1,
				TotalQuestions = 10,
				LastQuestionAnswered = false
			};

			SetSessionState(state);
		}

		public void ClearSession()
		{
			_httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
		}
	}

}
