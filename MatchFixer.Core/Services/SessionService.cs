using System.Text.Json;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
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
			return _httpContextAccessor.HttpContext?.Session.Get<GameSessionState>(SessionKey);
		}

		public void SetSessionState(GameSessionState state)
		{
			_httpContextAccessor.HttpContext?.Session.Set(SessionKey, state);
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

		public void SetBetSlipState(BetSlipState state)
		{
			_httpContextAccessor.HttpContext?.Session.Set(BetSlipSessionName, state);
		}

		public void ClearBetSlip()
		{
			_httpContextAccessor.HttpContext?.Session.Remove(BetSlipSessionName);
		}

		public BetSlipState? GetBetSlipState()
		{
			var stateJson = _httpContextAccessor.HttpContext?.Session.GetString(BetSlipSessionName);
			return stateJson == null ? null : JsonSerializer.Deserialize<BetSlipState>(stateJson);
		}

		public void AddBetToSlip(BetSlipItem item)
		{
			var betSlip = GetBetSlipState() ?? new BetSlipState();

			// Check if there is any existing bet for that match
			var existingBet = betSlip.Bets.FirstOrDefault(b => b.MatchId == item.MatchId);

			if (existingBet != null)
			{
				// Update the option & odds
				existingBet.SelectedOption = item.SelectedOption;
				existingBet.Odds = item.Odds;
				existingBet.HomeTeam = item.HomeTeam;
				existingBet.AwayTeam = item.AwayTeam;
				existingBet.HomeLogoUrl = item.HomeLogoUrl;
				existingBet.AwayLogoUrl = item.AwayLogoUrl;
			}
			else
			{
				betSlip.Bets.Add(item); // If bet does not exist, add it
			}

			SetBetSlipState(betSlip);
		}

		public void SetUserTimezone(string timezoneId)
		{
			_httpContextAccessor.HttpContext?.Session.SetString(UserTimezoneKey, timezoneId);
		}

		public string GetUserTimezone()
		{
			return _httpContextAccessor.HttpContext?.Session.GetString(UserTimezoneKey) ?? "UTC";
		}
	}

}
