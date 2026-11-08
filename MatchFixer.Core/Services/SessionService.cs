using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.Extensions;
using MatchFixer.Core.Helpers;
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
		private readonly IBettingLimitsService _limits;

		public SessionService(IHttpContextAccessor httpContextAccessor, MatchFixerDbContext dbContext, IOddsBoostService oddsBoostService,
			IBettingLimitsService limits)
		{
			_httpContextAccessor = httpContextAccessor;
			_dbContext = dbContext;
			_oddsBoostService = oddsBoostService;
			_limits = limits;
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

			// ── Extra markets: price comes from the MatchOdds table, one pick per match per market ──
			if (item.Market != BettingMarket.MatchResult)
			{
				var selection = BetMarketHelper.NormalizeSelection(item.Market, item.Selection ?? item.SelectedOption)
					?? throw new ArgumentException($"Invalid selection '{item.Selection ?? item.SelectedOption}' for {item.Market}.");

				if (item.Market == BettingMarket.DoubleChance && matchEvent.NoDraw)
					throw new InvalidOperationException("Double Chance is not available for this match.");

				if (!matchEvent.IsMarketEnabled(item.Market) || !(await _limits.GetMarketSwitchesAsync(ct)).IsEnabled(item.Market))
					throw new InvalidOperationException("This market is not available for this match.");

				var marketOdds = await _dbContext.MatchOdds
					.AsNoTracking()
					.FirstOrDefaultAsync(o => o.MatchEventId == item.MatchId
										   && o.Market == item.Market
										   && o.Selection == selection, ct)
					?? throw new InvalidOperationException("This market is not available for this match.");

				// One pick per match: a new pick replaces whatever was picked on this match (any market)
				var existingMarketBet = betSlip.Bets.FirstOrDefault(b => b.MatchId == item.MatchId);
				var target = existingMarketBet ?? new BetSlipItem { MatchId = item.MatchId };
				target.Market = item.Market;

				target.SelectedOption = selection; // codes are unique across markets, so this stays the slip key
				target.Selection = selection;
				target.Line = marketOdds.Line;
				target.Odds = marketOdds.Odds;
				target.HomeTeam = item.HomeTeam;
				target.AwayTeam = item.AwayTeam;
				target.HomeLogoUrl = item.HomeLogoUrl;
				target.AwayLogoUrl = item.AwayLogoUrl;
				target.StartTimeUtc = item.StartTimeUtc.HasValue
					? DateTime.SpecifyKind(item.StartTimeUtc.Value, DateTimeKind.Utc)
					: null;

				if (existingMarketBet == null)
					betSlip.Bets.Add(target);

				SetBetSlipState(betSlip);
				return;
			}

			// Get effective odds (including active boosts)
			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				matchEvent.Id,
				matchEvent.HomeOdds,
				matchEvent.DrawOdds,
				matchEvent.AwayOdds,
				ct
			);

			if (!Enum.TryParse<MatchPick>(item.SelectedOption, ignoreCase: true, out var pick))
				throw new ArgumentException($"Invalid SelectedOption: '{item.SelectedOption}'");

			decimal currentOdds = pick switch
			{
				MatchPick.Home => home ?? throw new InvalidOperationException("Home odds unavailable."),
				MatchPick.Draw => draw ?? throw new InvalidOperationException("Draw odds unavailable."),
				MatchPick.Away => away ?? throw new InvalidOperationException("Away odds unavailable."),
				_ => throw new ArgumentException($"Unhandled MatchPick value: {pick}")
			};

			// One pick per match: a new pick replaces whatever was picked on this match (any market)
			var existingBet = betSlip.Bets.FirstOrDefault(b => b.MatchId == item.MatchId);

			if (existingBet != null)
			{
				existingBet.Market = BettingMarket.MatchResult;
				existingBet.Selection = null;
				existingBet.Line = null;
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
