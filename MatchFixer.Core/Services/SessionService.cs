using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.Extensions;
using MatchFixer.Core.ViewModels.GameSessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MatchFixer.Infrastructure;
using static MatchFixer.Common.GeneralConstants.SessionConstants;

namespace MatchFixer.Core.Services
{
	public class SessionService : ISessionService
	{
		private const string SessionKey = GameSessionName;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly MatchFixerDbContext _dbContext;
		private readonly IOddsBoostService _oddsBoostService;

		public SessionService(IHttpContextAccessor httpContextAccessor, MatchFixerDbContext dbContext, IOddsBoostService oddsBoostService)
		{
			_httpContextAccessor = httpContextAccessor;
			_dbContext = dbContext;
			_oddsBoostService = oddsBoostService;
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
			return _httpContextAccessor.HttpContext?.Session.Get<BetSlipState>(BetSlipSessionName);
		}


		public async Task AddBetToSlipAsync(BetSlipItem item, CancellationToken ct = default)
		{
			var betSlip = GetBetSlipState() ?? new BetSlipState();

			// Fetch the match (just to confirm it exists & get base odds)
			var matchEvent = await _dbContext.MatchEvents
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.Id == item.MatchId, ct);

			if (matchEvent == null)
				throw new InvalidOperationException($"Match with ID {item.MatchId} not found.");

			// Get effective odds (including active boosts)
			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				matchEvent.Id,
				matchEvent.HomeOdds,
				matchEvent.DrawOdds,
				matchEvent.AwayOdds,
				ct
			);

			decimal currentOdds = item.SelectedOption switch
			{
				"Home" => home ?? throw new InvalidOperationException("Home odds unavailable."),
				"Draw" => draw ?? throw new InvalidOperationException("Draw odds unavailable."),
				"Away" => away ?? throw new InvalidOperationException("Away odds unavailable."),
				_ => throw new ArgumentException("Invalid SelectedOption")
			};

			// Update existing bet or add a new one
			var existingBet = betSlip.Bets.FirstOrDefault(b => b.MatchId == item.MatchId);

			if (existingBet != null)
			{
				existingBet.SelectedOption = item.SelectedOption;
				existingBet.Odds = currentOdds;
				existingBet.HomeTeam = item.HomeTeam;
				existingBet.AwayTeam = item.AwayTeam;
				existingBet.HomeLogoUrl = item.HomeLogoUrl;
				existingBet.AwayLogoUrl = item.AwayLogoUrl;
				existingBet.StartTimeUtc = item.StartTimeUtc.HasValue
					? DateTime.SpecifyKind(item.StartTimeUtc.Value, DateTimeKind.Utc)
					: null;
			}
			else
			{
				var newBet = new BetSlipItem
				{
					MatchId = item.MatchId,
					SelectedOption = item.SelectedOption,
					Odds = currentOdds,
					HomeTeam = item.HomeTeam,
					AwayTeam = item.AwayTeam,
					HomeLogoUrl = item.HomeLogoUrl,
					AwayLogoUrl = item.AwayLogoUrl,
					StartTimeUtc = item.StartTimeUtc.HasValue
						? DateTime.SpecifyKind(item.StartTimeUtc.Value, DateTimeKind.Utc)
						: null
				};

				betSlip.Bets.Add(newBet);
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
