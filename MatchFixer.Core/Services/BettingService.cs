using MatchFixer.Common.Enums;
using MatchFixer.Common.EmailTemplates;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.DTOs.Limits;
using MatchFixer.Core.Helpers;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.EmailTemplates.EmailTemplates;
using static MatchFixer.Common.GeneralConstants.BettingServiceConstants;
using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;

namespace MatchFixer.Core.Services;

public class BettingService : IBettingService
{
	private readonly MatchFixerDbContext _dbContext;
	private readonly ISessionService _sessionService;
	private readonly ITimezoneService _timezoneService;
	private readonly IWalletService _walletService;
	private readonly ITrophyService _trophyService;
	private readonly IOddsBoostService _oddsBoostService;
	private readonly IAdminInsightsNotifier _notifier;
	private readonly IEmailSender _emailSender;
	private readonly IEmailQueueService _emailQueue;
	private readonly IBettingEligibilityService _eligibility;
	private readonly IBettingLimitsService _limits;


	public BettingService(MatchFixerDbContext dbContext,
		ISessionService sessionService,
		ITimezoneService timezoneService,
		IWalletService walletService,
		ITrophyService trophyService,
		IOddsBoostService oddsBoostService,
		IAdminInsightsNotifier notifier,
		IEmailSender emailSender,
		IEmailQueueService emailQueue,
		IBettingEligibilityService eligibility,
		IBettingLimitsService limits
		)
	{
		_dbContext = dbContext;
		_sessionService = sessionService;
		_timezoneService = timezoneService;
		_walletService = walletService;
		_trophyService = trophyService;
		_oddsBoostService = oddsBoostService;
		_notifier = notifier;
		_emailSender = emailSender;
		_emailQueue = emailQueue;
		_eligibility = eligibility;
		_limits = limits;
	}

	public async Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto, string profileUrl, CancellationToken ct = default)
	{
		var eligibility = await _eligibility.GetEligibilityAsync(userId, ct);
		if (!eligibility.CanBet)
			return (eligibility.Message, false);

		var now = DateTime.UtcNow;

		var recent = await _dbContext.BetSlips
			.Include(x => x.Bets)
			.Where(x => x.UserId == userId)
			.Where(x => x.BetTime > now.AddSeconds(-3))
			.OrderByDescending(x => x.BetTime)
			.FirstOrDefaultAsync();

		if (recent != null)
		{
			bool isSame =
				recent.Amount == betSlipDto.Amount &&
				recent.Bets.Count == betSlipDto.Bets.Count &&
				!recent.Bets.Any(b =>
					!betSlipDto.Bets.Any(dto =>
						dto.MatchId == b.MatchEventId &&
						dto.Market == b.Market &&
						(b.Market == BettingMarket.MatchResult
							? string.Equals(dto.SelectedOption, b.Pick.ToString(), StringComparison.OrdinalIgnoreCase)
							: string.Equals(dto.Selection ?? dto.SelectedOption, b.Selection, StringComparison.OrdinalIgnoreCase))
					));

			if (isSame)
			{
				return (DuplicateBetDetected, false);
			}
		}

		if (betSlipDto == null || betSlipDto.Bets == null || !betSlipDto.Bets.Any())
			return (NoBetsProvided, false);

		if (betSlipDto.Amount <= 0)
			return (BetAmountMustBeGreaterThanZero, false);

		// One pick per match on a slip (any market)
		if (betSlipDto.Bets.GroupBy(b => b.MatchId).Any(g => g.Count() > 1))
			return ("Only one pick per match is allowed on a slip.", false);

		var betSlip = new BetSlip
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Amount = betSlipDto.Amount,
			BetTime = DateTime.UtcNow,
			IsSettled = false
		};

		// Deduct once for the whole slip
		var (success, message) = await _walletService.DeductForBetAsync(userId, betSlipDto.Amount, betSlip.Id);
		if (!success)
			return (message, false);

		var affectedEventIds = new HashSet<Guid>();

		foreach (var betDto in betSlipDto.Bets)
		{
			var matchEvent = await _dbContext.MatchEvents
				.AsTracking()
				.FirstOrDefaultAsync(x => x.Id == betDto.MatchId); 
			
			if (matchEvent == null)
				return (InvalidMatchId(betDto.MatchId), false);

			var blockReason = GetBettingBlockReason(matchEvent);

			if (blockReason != BettingBlockReason.None)
			{
				return blockReason switch
				{
					BettingBlockReason.Cancelled =>
						(EventCancelledInSlip, false),

					BettingBlockReason.Postponed =>
						(EventPostponedInSlip, false),

					BettingBlockReason.AlreadyStarted =>
						(EventAlreadyStartedInSlip, false),

					BettingBlockReason.AlreadyFinished =>
						(EventAlreadyFinishedInSlip, false),

					_ =>
						(EventNotOpenForBetting, false)
				};
			}


			if (DateTime.UtcNow >= matchEvent.MatchDate)
				return (EventAlreadyStartedInSlip, false);

			// ── Extra markets (Over/Under, BTTS, Double Chance) ──
			if (betDto.Market != BettingMarket.MatchResult)
			{
				var selection = BetMarketHelper.NormalizeSelection(betDto.Market, betDto.Selection ?? betDto.SelectedOption);
				if (selection == null)
					return (InvalidPickOption(betDto.Selection ?? betDto.SelectedOption), false);

				// No Draw matches have no draw outcome, so Double Chance is not offered on them
				if (betDto.Market == BettingMarket.DoubleChance && matchEvent.NoDraw)
					return ("Double Chance is not available for this match.", false);

				// Market switched off site-wide (Limits page) or for this match (Manage Matches)
				if (!matchEvent.IsMarketEnabled(betDto.Market) || !(await _limits.GetMarketSwitchesAsync(ct)).IsEnabled(betDto.Market))
					return ("This market is not available for this match.", false);

				var marketOdds = await _dbContext.MatchOdds
					.AsNoTracking()
					.FirstOrDefaultAsync(o => o.MatchEventId == matchEvent.Id
										   && o.Market == betDto.Market
										   && o.Selection == selection, ct);

				if (marketOdds == null)
					return ("This market is not available for this match.", false);

				betSlip.Bets.Add(new Bet
				{
					Id = Guid.NewGuid(),
					BetSlipId = betSlip.Id,
					MatchEventId = matchEvent.Id,
					Market = betDto.Market,
					Selection = selection,
					Line = marketOdds.Line,
					Pick = default, // not used for extra markets
					Odds = marketOdds.Odds,
					BetTime = DateTime.UtcNow
				});

				affectedEventIds.Add(matchEvent.Id);
				continue;
			}

			if (!Enum.TryParse<MatchPick>(betDto.SelectedOption, true, out var parsedPick))
				return (InvalidPickOption(betDto.SelectedOption), false);

			// Reject draw pick when the match has no-draw enabled
			if (parsedPick == MatchPick.Draw && matchEvent.NoDraw)
				return ("Draw is not available for this match.", false);

			// Use the shared odds calculator
			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				matchEvent.Id,
				matchEvent.HomeOdds,
				matchEvent.DrawOdds,
				matchEvent.AwayOdds
			);

			decimal? effectiveOdds = parsedPick switch
			{
				MatchPick.Home => home,
				MatchPick.Draw => draw,
				MatchPick.Away => away,
				_ => betDto.Odds // fallback, shouldn't happen if validated
			};

			if (boost != null)
			{
				// Enforce max stake
				if (boost.MaxStakePerBet.HasValue && betSlipDto.Amount > boost.MaxStakePerBet.Value)
					return (MaxStakePerBetIs(boost.MaxStakePerBet.Value), false);

				// Enforce max uses per user
				if (boost.MaxUsesPerUser.HasValue)
				{
					var used = await _dbContext.Bets
						.CountAsync(x => x.BetSlip.UserId == userId &&
										 x.MatchEventId == matchEvent.Id &&
										 x.OddsBoostId == boost.Id);

					if (used >= boost.MaxUsesPerUser.Value)
						return (BoostAlreadyUsedMaximumTimesForEvent, false);
				}
			}

			var bet = new Bet
			{
				Id = Guid.NewGuid(),
				BetSlipId = betSlip.Id,
				MatchEventId = matchEvent.Id,
				Market = BettingMarket.MatchResult,
				Pick = parsedPick,
				Odds = (decimal)effectiveOdds,
				BetTime = DateTime.UtcNow,
				OddsBoostId = boost?.Id
			};

			betSlip.Bets.Add(bet);
			affectedEventIds.Add(matchEvent.Id);

		}

		// Stake / win limits (admin Limits page): league x legs, averaged across the slip's leagues
		var slipOdds = betSlip.Bets.Aggregate(1m, (acc, b) => acc * b.Odds);
		var limits = await _limits.GetSlipLimitsAsync(
			betSlip.Bets.Select(b => new SlipLeg(b.MatchEventId, b.Market)).ToList(), slipOdds, ct);

		// A capped odds-boost promo on the slip overrides the league limits and max win
		if (limits.EffectiveMaxStake.HasValue && betSlipDto.Amount > limits.EffectiveMaxStake.Value)
		{
			var reason = limits.PromoMaxStake.HasValue
				? " (the promo on this slip sets the limit)"
				: limits.MaxStakeForWin.HasValue && limits.EffectiveMaxStake == limits.MaxStakeForWin
					? $" (max win for this slip is €{limits.MaxWin:0.00})"
					: string.Empty;

			return ($"The maximum stake for this slip is €{limits.EffectiveMaxStake.Value:0.00}{reason}.", false);
		}

		await _dbContext.BetSlips.AddAsync(betSlip);
		await _dbContext.SaveChangesAsync();

		await PublishBetMixForEventsAsync(affectedEventIds, ct);

		// await _trophyService.EvaluateTrophiesAsync(userId);

		return (BetSlipSubmittedSuccessfully, true);
	}


	public async Task<IEnumerable<UserBetSlipDTO>> GetBetsByUserAsync(Guid userId)
	{
		var betSlips = await _dbContext.BetSlips
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.HomeTeam)
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.AwayTeam)
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.LiveResult)
			.Where(bs => bs.UserId == userId)
			.Where(bs => bs.TournamentEntryId == null) // tournament bets live on the tournament page, not here
			.OrderByDescending(bs => bs.BetTime)
			.AsNoTracking()
			.ToListAsync();

		var timeZoneId = _sessionService.GetUserTimezone();

		return betSlips.Select(bs =>
		{
			var slipStatus = ComputeSlipStatus(bs);
			var totalOdds = ComputeTotalOdds(bs.Bets);

			return new UserBetSlipDTO
			{
				Id = bs.Id,
				BetTime = bs.BetTime,
				DisplayTime = _timezoneService.FormatForUserBets(bs.BetTime, timeZoneId),
				Amount = bs.Amount,
				UserId = bs.UserId,
				WinAmount = bs.WinAmount,
				TotalOdds = totalOdds,
				Status = slipStatus.ToString(),

				Bets = bs.Bets.Select(b =>
				{
					var computedStatus = ResolveBetStatus(b);
					var result = b.MatchEvent.LiveResult;

					string? outcome = result == null
						? null
						: result.HomeWonOnPenalties.HasValue
							? (result.HomeWonOnPenalties.Value ? MatchPick.Home.ToString() : MatchPick.Away.ToString())
							: result.HomeScore > result.AwayScore
								? MatchPick.Home.ToString()
								: result.HomeScore < result.AwayScore
									? MatchPick.Away.ToString()
									: MatchPick.Draw.ToString();

					return new SingleBetDto
					{
						MatchId = b.MatchEventId,
						HomeTeam = b.MatchEvent.HomeTeam.Name,
						AwayTeam = b.MatchEvent.AwayTeam.Name,
						SelectedOption = BetMarketHelper.DisplayPick(b),
						Market = b.Market,
						Selection = b.Selection,
						Line = b.Line,
						Odds = b.Odds,
						Outcome = b.Market == BettingMarket.MatchResult || result == null
							? outcome
							: $"{result.HomeScore}-{result.AwayScore}",
						Status = computedStatus.ToString()
					};
				}).ToList()
			};
		});
	}


	public async Task<PagedBetSlipsDTO> GetBetSlipsPageAsync(Guid userId, string status, int page, int pageSize)
		{
			if (page < 1) page = 1;
			if (pageSize < 1) pageSize = 15;

			var all = (await GetBetsByUserAsync(userId))
				.Where(x => x.Status == status)
				.ToList();

			var summary = status switch
			{
				nameof(BetStatus.Pending) => all.Sum(x => x.Amount * x.TotalOdds),
				nameof(BetStatus.Won)     => all.Sum(x => x.WinAmount ?? 0),
				nameof(BetStatus.Lost)    => all.Sum(x => x.Amount),
				nameof(BetStatus.Voided)  => all.Sum(x => x.Amount),
				_                         => 0m
			};

			var pageItems = all
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PagedBetSlipsDTO
			{
				Status = status,
				Slips = pageItems,
				Page = page,
				PageSize = pageSize,
				TotalCount = all.Count,
				SummaryAmount = summary
			};
		}

		private static BetStatus ComputeSlipStatus(BetSlip slip)
	{
		var statuses = slip.Bets
			.Select(ResolveBetStatus)
			.ToList();

		if (statuses.Any(s => s == BetStatus.Voided))
			return BetStatus.Voided;

		if (statuses.Any(s => s == BetStatus.Lost))
			return BetStatus.Lost;

		if (statuses.All(s => s == BetStatus.Won))
			return BetStatus.Won;

		return BetStatus.Pending;
	}

	private static BetStatus ResolveBetStatus(Bet b)
		=> BetMarketHelper.ResolveBetStatus(b); // handles cancelled / pending / every market


	static decimal ComputeTotalOdds(IEnumerable<Bet> bets)
	{
		return bets
			.Where(b => b.Status != BetStatus.Voided)
			.Aggregate(1m, (acc, b) => acc * b.Odds);
	}

	public async Task<bool> CancelBetsForMatchAsync(Guid matchEventId)
	{
		var bets = await _dbContext.Bets
			.Where(b => b.MatchEventId == matchEventId)
			.ToListAsync();

		if (!bets.Any())
			return true;

		foreach (var bet in bets)
		{
			bet.Status = BetStatus.Voided;
		}

		await _dbContext.SaveChangesAsync();

		var affectedSlipIds = bets
			.Select(b => b.BetSlipId)
			.Distinct()
			.ToList();

		foreach (var slipId in affectedSlipIds)
		{
			await EvaluateBetSlipAsync(slipId);
		}

		return true;
	}


	public async Task<bool> EvaluateBetSlipAsync(Guid betSlipId)
	{
		var betSlip = await _dbContext.BetSlips
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.LiveResult)
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.HomeTeam)
			.Include(bs => bs.Bets)
				.ThenInclude(b => b.MatchEvent)
					.ThenInclude(me => me.AwayTeam)
			.FirstOrDefaultAsync(bs => bs.Id == betSlipId);

		if (betSlip == null)
			return false;

		if (betSlip.IsSettled)
		{
			if (betSlip.SettledAt == null)
			{
				betSlip.SettledAt = DateTime.UtcNow;
				await _dbContext.SaveChangesAsync();
			}

			return true;
		}

		// Tournament slips are funded by the entry's play bankroll, not the real wallet.
		// When isTournament is true we move chips on the entry and skip wallet moves,
		// settlement emails and trophy evaluation. The real-wallet path below is unchanged.
		TournamentEntry? tournamentEntry = null;
		if (betSlip.TournamentEntryId.HasValue)
		{
			tournamentEntry = await _dbContext.TournamentEntries
				.FirstOrDefaultAsync(e => e.Id == betSlip.TournamentEntryId.Value);
		}
		bool isTournament = tournamentEntry != null;

		bool anyLost = false;
		bool anyPending = false;
		bool anyVoided = false;
		decimal totalOdds = 1m;

		foreach (var bet in betSlip.Bets)
		{
			// Voided bets stay voided
			if (bet.Status == BetStatus.Voided)
			{
				anyVoided = true;
				continue;
			}

			var result = bet.MatchEvent.LiveResult;

			// No result yet → pending
			if (result == null)
			{
				bet.Status = BetStatus.Pending;
				anyPending = true;
				continue;
			}

			if (BetMarketHelper.IsWinningBet(bet, result.HomeScore, result.AwayScore, result.HomeWonOnPenalties))
			{
				bet.Status = BetStatus.Won;
				totalOdds *= bet.Odds;
			}
			else
			{
				bet.Status = BetStatus.Lost;
				anyLost = true;
			}
		}

		// --- SLIP STATUS LOGIC ---

		// ANY VOID → VOID SLIP
		if (anyVoided)
		{
			betSlip.IsSettled = true;
			betSlip.SettledAt = DateTime.UtcNow;
			betSlip.WinAmount = 0;

			if (isTournament)
			{
				// Return the stake to the play bankroll. No wallet, email or trophy.
				tournamentEntry!.Bankroll += betSlip.Amount;
				await _dbContext.SaveChangesAsync();
				return true;
			}

			var alreadyRefunded =
				await _walletService.HasTransactionForSlipAsync(
					betSlip.UserId,
					WalletTransactionType.Refund,
					betSlip.Id
				);

			if (!alreadyRefunded)
			{
				await _walletService.RefundBetAsync(
					betSlip.UserId,
					betSlip.Amount,
					betSlip.Id
				);
			}

			await _dbContext.SaveChangesAsync();
			await _trophyService.EvaluateTrophiesAsync(betSlip.UserId);
			await SendSettlementEmailAsync(betSlip, "Voided", refundAmount: betSlip.Amount);

			return true;
		}

		// ANY LOST + NO PENDING → LOST SLIP
		if (anyLost && !anyPending)
		{
			betSlip.IsSettled = true;
			betSlip.SettledAt = DateTime.UtcNow;
			betSlip.WinAmount = 0;

			await _dbContext.SaveChangesAsync();

			// Tournament slips: stake already left the bankroll at placement, nothing to move.
			if (!isTournament)
				await _trophyService.EvaluateTrophiesAsync(betSlip.UserId);

			return true;
		}

		// ALL WON → WON SLIP
		if (!anyLost && !anyPending)
		{
			var winnings = Math.Round(betSlip.Amount * totalOdds, 2);

			betSlip.IsSettled = true;
			betSlip.SettledAt = DateTime.UtcNow;
			betSlip.WinAmount = winnings;

			if (isTournament)
			{
				// Credit winnings to the play bankroll. No wallet, email or trophy.
				tournamentEntry!.Bankroll += winnings;
				await _dbContext.SaveChangesAsync();
				return true;
			}

			var alreadyPaid =
				await _walletService.HasTransactionForSlipAsync(
					betSlip.UserId,
					WalletTransactionType.Winnings,
					betSlip.Id
				);

			if (!alreadyPaid)
			{
				await _walletService.AwardWinningsAsync(
					betSlip.UserId,
					winnings,
					$"BetSlip # {betSlip.Id}"
				);

			}

			await _dbContext.SaveChangesAsync();
			await _trophyService.EvaluateTrophiesAsync(betSlip.UserId);
			await SendSettlementEmailAsync(betSlip, "Won", refundAmount: winnings);

			return true;

		}

		// OTHERWISE → STILL PENDING
		betSlip.IsSettled = false;
		betSlip.WinAmount = null;

		await _dbContext.SaveChangesAsync();
		return false;
	}


	private static BettingBlockReason GetBettingBlockReason(MatchEvent ev)
	{
		if (ev == null)
			return BettingBlockReason.Cancelled;

		if (ev.IsCancelled)
			return BettingBlockReason.Cancelled;

		if (ev.IsPostponed)
			return BettingBlockReason.Postponed;

		if (ev.LiveResult != null)
			return BettingBlockReason.AlreadyFinished;

		if (ev.MatchDate <= DateTime.UtcNow)
			return BettingBlockReason.AlreadyStarted;

		return BettingBlockReason.None;
	}



	private async Task SendSettlementEmailAsync(BetSlip betSlip, string outcome, decimal? refundAmount)
	{
		try
		{
			var user = await _dbContext.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == betSlip.UserId);

			if (user?.Email == null)
				return;

			var totalOdds = betSlip.Bets
				.Where(b => b.Status != BetStatus.Voided)
				.Aggregate(1m, (acc, b) => acc * b.Odds);

			var betRows = betSlip.Bets.Select(b => new BetRowEmailData(
				HomeTeam: b.MatchEvent?.HomeTeam?.Name ?? "Home",
				AwayTeam: b.MatchEvent?.AwayTeam?.Name ?? "Away",
				Pick: BetMarketHelper.DisplayPick(b),
				Status: b.Status.ToString()
			));

			var subject = outcome switch
			{
				"Won"    => SubjectBetSlipWon,
				"Voided" => SubjectBetSlipVoided,
				_        => SubjectBetSlipVoided
			};

			var body = BetSlipSettledEmail(
				logoUrl: LogoUrl,
				userName: user.FullName,
				outcome: outcome,
				stake: betSlip.Amount,
				totalOdds: totalOdds,
				winAmount: refundAmount,
				bets: betRows
			);

			var category = outcome == "Won"
				? EmailQueueCategory.Winning
				: EmailQueueCategory.Voided;

			await _emailQueue.EnqueueAsync(user.Email, subject, body, category, user.Id, $"Slip {betSlip.Id:N}");
		}
		catch
		{
			// Email failure must never break settlement
		}
	}

	private async Task PublishBetMixForEventsAsync(IEnumerable<Guid> eventIds, CancellationToken ct)
	{
		foreach (var eventId in eventIds.Distinct())
		{
			var q = _dbContext.Bets
				.AsNoTracking()
				.Where(b => b.MatchEventId == eventId
						 && b.Status == BetStatus.Pending
						 && b.BetSlip.TournamentEntryId == null); // exclude tournament bankroll bets from the public mix

			// The public mix keeps the headline numbers pure 1X2 (extra markets are
			// summarised separately in the admin accordion), so Total Bets / Total Stake
			// here count Match Result legs only.
			var q1x2 = q.Where(b => b.Market == BettingMarket.MatchResult);

			var perLegStakes = await q1x2
				.Select(b => b.BetSlip.Amount / b.BetSlip.Bets.Count)
				.ToListAsync(ct);

			var totalStake = perLegStakes.Sum(); // in-memory sum (safe)

			var total1x2 = await q1x2.CountAsync(ct);
			var total = total1x2;
			var h = await q1x2.CountAsync(b => b.Pick == MatchPick.Home, ct);
			var d = await q1x2.CountAsync(b => b.Pick == MatchPick.Draw, ct);
			var a = await q1x2.CountAsync(b => b.Pick == MatchPick.Away, ct);

			decimal hp = 0, dp = 0, ap = 0;
			if (total1x2 > 0)
			{
				hp = Math.Round(100m * h / total1x2, 2);
				dp = Math.Round(100m * d / total1x2, 2);
				ap = Math.Round(100m * a / total1x2, 2);

				var drift = 100m - (hp + dp + ap);
				if (drift != 0)
				{
					if (hp >= dp && hp >= ap) hp += drift;
					else if (dp >= ap) dp += drift;
					else ap += drift;
				}
			}

			await _notifier.PublishBetMixAsync(
				new BetMixUpdateDto { EventId = eventId, TotalBets = total, TotalStake = totalStake, HomePct = hp, DrawPct = dp, AwayPct = ap },
				ct);
		}
	}



}