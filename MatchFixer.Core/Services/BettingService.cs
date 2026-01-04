using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Common.GeneralConstants.BettingServiceConstants;

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


	public BettingService(MatchFixerDbContext dbContext, 
		ISessionService sessionService, 
		ITimezoneService timezoneService,
		IWalletService walletService,
		ITrophyService trophyService,
		IOddsBoostService oddsBoostService,
		IAdminInsightsNotifier notifier
		)
	{
		_dbContext = dbContext;
		_sessionService = sessionService;
		_timezoneService = timezoneService;
		_walletService = walletService;
		_trophyService = trophyService;
		_oddsBoostService = oddsBoostService;
		_notifier = notifier;

	}

	public async Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto, string profileUrl, CancellationToken ct = default)
	{
		if (betSlipDto == null || betSlipDto.Bets == null || !betSlipDto.Bets.Any())
			return (NoBetsProvided, false);

		if (betSlipDto.Amount <= 0)
			return (BetAmountMustBeGreaterThanZero, false);

		var betSlip = new BetSlip
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Amount = betSlipDto.Amount,
			BetTime = DateTime.UtcNow,
			IsSettled = false
		};

		// Deduct once for the whole slip
		var (success, message) = await _walletService.DeductForBetAsync(userId, betSlipDto.Amount);
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

			if (!IsEventOpenForBetting(matchEvent))
				return (EventOrEventsCancelledInSlip, false);

			if (DateTime.UtcNow >= matchEvent.MatchDate)
				return (EventAlreadyStartedInSlip, false);

			if (!Enum.TryParse<MatchPick>(betDto.SelectedOption, true, out var parsedPick))
				return (InvalidPickOption(betDto.SelectedOption), false);

			// Use the shared odds calculator
			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				matchEvent.Id,
				matchEvent.HomeOdds,
				matchEvent.DrawOdds,
				matchEvent.AwayOdds
			);

			decimal? effectiveOdds = betDto.SelectedOption.ToLower() switch
			{
				"home" => home,
				"draw" => draw,
				"away" => away,
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
				Pick = parsedPick,
				Odds = (decimal)effectiveOdds,
				BetTime = DateTime.UtcNow,
				OddsBoostId = boost?.Id
			};

			betSlip.Bets.Add(bet);
			affectedEventIds.Add(matchEvent.Id);

		}

		await _dbContext.BetSlips.AddAsync(betSlip);
		await _dbContext.SaveChangesAsync();

		await PublishBetMixForEventsAsync(affectedEventIds, ct);

		// await _trophyService.EvaluateTrophiesAsync(userId, profileUrl);

		return (BetSlipSubmittedSuccessfully, true);
	}



	//public async Task<IEnumerable<UserBetSlipDTO>> GetBetsByUserAsync(Guid userId)
	//{
	//	var betSlips = await _dbContext.BetSlips
	//		.Include(bs => bs.Bets)
	//		.ThenInclude(b => b.MatchEvent)
	//		.ThenInclude(me => me.HomeTeam)
	//		.Include(bs => bs.Bets)
	//		.ThenInclude(b => b.MatchEvent)
	//		.ThenInclude(me => me.AwayTeam)
	//		.Include(bs => bs.Bets)
	//		.ThenInclude(b => b.MatchEvent)
	//		.ThenInclude(me => me.LiveResult) 
	//		.Where(bs => bs.UserId == userId)
	//		.OrderByDescending(b => b.BetTime)
	//		.ToListAsync();


	//	foreach (var slip in betSlips)
	//	{
	//		slip.Bets = slip.Bets
	//			.Where(b => b.Status != BetStatus.Voided)
	//			.ToList();

	//	}

	//	betSlips = betSlips
	//		.Where(bs => bs.Bets.Any())
	//		.ToList();

	//	var timeZoneId = _sessionService.GetUserTimezone();



	//	return betSlips.Select(bs => new UserBetSlipDTO
	//	{
	//		Id = bs.Id,
	//		BetTime = bs.BetTime,
	//		DisplayTime = _timezoneService.FormatForUserBets(bs.BetTime, timeZoneId),
	//		Amount = bs.Amount,
	//		UserId = bs.UserId,
	//		WinAmount = bs.WinAmount,
	//		TotalOdds = bs.Bets.Any()
	//			? bs.Bets.Select(b => b.Odds).Aggregate((acc, odd) => acc * odd)
	//			: 1,
	//		Status = bs.IsSettled
	//			? (bs.WinAmount > 0 ? nameof(BetStatus.Won) : nameof(BetStatus.Lost))
	//			: nameof(BetStatus.Pending),

	//		Bets = bs.Bets.Select(b =>
	//		{
	//			var result = b.MatchEvent.LiveResult;
	//			string? outcome = null;

	//			if (result != null)
	//			{
	//				outcome = result.HomeScore > result.AwayScore
	//					? MatchPick.Home.ToString()
	//					: result.HomeScore < result.AwayScore
	//						? MatchPick.Away.ToString()
	//						: MatchPick.Draw.ToString();
	//			}

	//			return new SingleBetDto
	//			{
	//				MatchId = b.MatchEventId,
	//				HomeTeam = b.MatchEvent.HomeTeam.Name,
	//				AwayTeam = b.MatchEvent.AwayTeam.Name,
	//				SelectedOption = b.Pick.ToString(),
	//				Odds = b.Odds,
	//				Outcome = outcome
	//			};
	//		}).ToList()
	//	});
	//}

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
			.OrderByDescending(bs => bs.BetTime)
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
					var result = b.MatchEvent.LiveResult;
					string? outcome = null;

					if (result != null)
					{
						outcome =
							result.HomeScore > result.AwayScore ? MatchPick.Home.ToString() :
							result.HomeScore < result.AwayScore ? MatchPick.Away.ToString() :
							MatchPick.Draw.ToString();
					}

					return new SingleBetDto
					{
						MatchId = b.MatchEventId,
						HomeTeam = b.MatchEvent.HomeTeam.Name,
						AwayTeam = b.MatchEvent.AwayTeam.Name,
						SelectedOption = b.Pick.ToString(),
						Odds = b.Odds,
						Outcome = outcome,
						Status = b.Status.ToString() 
					};
				}).ToList()
			};
		});
	}


	static BetStatus ComputeSlipStatus(BetSlip slip)
	{
		if (slip.Bets.Any(b => b.Status == BetStatus.Voided))
			return BetStatus.Voided;

		if (slip.Bets.Any(b => b.Status == BetStatus.Lost))
			return BetStatus.Lost;

		if (slip.Bets.Any(b => b.Status == BetStatus.Pending))
			return BetStatus.Pending;

		return BetStatus.Won;
	}


	static decimal ComputeTotalOdds(IEnumerable<Bet> bets)
	{
		return bets
			.Where(b => b.Status != BetStatus.Voided)
			.Aggregate(1m, (acc, b) => acc * b.Odds);
	}

	public async Task<bool> CancelBetsForMatchAsync(Guid matchEventId)
	{
		var relatedBets = await _dbContext.Bets
			.Include(b => b.BetSlip)
			.Where(b => b.MatchEventId == matchEventId && b.Status == BetStatus.Pending)
			.ToListAsync();

		if (!relatedBets.Any())
			return true;

		var betSlipGroups = relatedBets.GroupBy(b => b.BetSlipId);

		foreach (var group in betSlipGroups)
		{
			var betSlip = group.First().BetSlip;

			// Void only the bets for the canceled match
			foreach (var bet in group)
				bet.Status = BetStatus.Voided;

			// Load all bets for this slip (regardless of status)
			var allBetsInSlip = await _dbContext.Bets
				.Where(b => b.BetSlipId == betSlip.Id)
				.ToListAsync();

			bool anyVoided = allBetsInSlip.Any(b => b.Status == BetStatus.Voided);
			bool anyPending = allBetsInSlip.Any(b => b.Status == BetStatus.Pending);
			bool allWon = allBetsInSlip.All(b => b.Status == BetStatus.Won || b.Status == BetStatus.Voided);
			bool anyLost = allBetsInSlip.Any(b => b.Status == BetStatus.Lost);

			if (!anyPending)
			{
				// Settle the slip because no more pending bets
				betSlip.IsSettled = true;

				if (anyVoided)
				{
					betSlip.WinAmount = 0;
					await _walletService.RefundBetAsync(betSlip.UserId, betSlip.Amount, betSlip.Id);
				}
				else if (allWon && !anyLost)
				{
					// Calculate winnings from only the WON bets (skip voided ones)
					var totalOdds = allBetsInSlip
						.Where(b => b.Status == BetStatus.Won)
						.Aggregate(1m, (acc, bet) => acc * bet.Odds);

					betSlip.WinAmount = Math.Round(betSlip.Amount * totalOdds, 2);

					await _walletService.AwardWinningsAsync(betSlip.UserId, betSlip.WinAmount.Value, betSlip.Id.ToString());
				}
				else
				{
					// Some bet lost, no win
					betSlip.WinAmount = 0;
				}
			}
		}

		await _dbContext.SaveChangesAsync();
		return true;
	}


	public async Task<bool> EvaluateBetSlipAsync(Guid betSlipId)
	{
		var betSlip = await _dbContext.BetSlips
			.Include(bs => bs.Bets)
			.ThenInclude(b => b.MatchEvent)
			.ThenInclude(me => me.LiveResult)
			.FirstOrDefaultAsync(bs => bs.Id == betSlipId);

		if (betSlip == null || betSlip.IsSettled)
			return false;

		// ANY VOIDED → VOID ENTIRE SLIP
		if (betSlip.Bets.Any(b => b.Status == BetStatus.Voided))
		{
			betSlip.IsSettled = true;
			betSlip.WinAmount = 0;

			await _walletService.RefundBetAsync(
				betSlip.UserId,
				betSlip.Amount,
				betSlip.Id
			);

			await _dbContext.SaveChangesAsync();
			return true;
		}

		bool anyLost = false;
		bool anyPending = false;
		decimal totalOdds = 1m;

		foreach (var bet in betSlip.Bets)
		{
			var result = bet.MatchEvent.LiveResult;

			if (result == null)
			{
				anyPending = true;
				continue;
			}

			var actualOutcome =
				result.HomeScore > result.AwayScore ? MatchPick.Home :
				result.HomeScore < result.AwayScore ? MatchPick.Away :
				MatchPick.Draw;

			if (bet.Pick == actualOutcome)
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

		// ANY LOST → LOST SLIP (IMMEDIATE)
		if (anyLost)
		{
			betSlip.IsSettled = true;
			betSlip.WinAmount = 0;

			await _dbContext.SaveChangesAsync();
			return true;
		}

		// NO LOST BUT SOME PENDING → DO NOT SETTLE
		if (anyPending)
			return false;

		// ALL WON → WON SLIP
		betSlip.IsSettled = true;
		betSlip.WinAmount = Math.Round(betSlip.Amount * totalOdds, 2);

		await _walletService.AwardWinningsAsync(
			betSlip.UserId,
			betSlip.WinAmount.Value,
			betSlip.Id.ToString()
		);

		await _dbContext.SaveChangesAsync();
		await _trophyService.EvaluateTrophiesAsync(betSlip.UserId);

		return true;
	}





	private static bool IsEventOpenForBetting(MatchEvent ev)
	{
		var nowUtc = DateTime.UtcNow;

		// if event was not found return false
		if (ev == null) return false;

		// if event is cancelled return false
		if (ev.IsCancelled) return false;

		// if that event already has a result recorded in the database return false
		if (ev.LiveResult != null) return false;

		return true;
	}


	private async Task PublishBetMixForEventsAsync(IEnumerable<Guid> eventIds, CancellationToken ct)
	{
		foreach (var eventId in eventIds.Distinct())
		{
			var q = _dbContext.Bets
				.AsNoTracking()
				.Where(b => b.MatchEventId == eventId && b.Status == BetStatus.Pending);

			var perSlip = await q
				.GroupBy(b => b.BetSlipId)
				.Select(g => new
				{
					PerLeg = g.Select(x => x.BetSlip.Amount).FirstOrDefault() / (decimal)g.Count()
				})
				.ToListAsync(ct);

			var totalStake = perSlip.Sum(x => x.PerLeg); // in-memory sum (safe)

			var total = await q.CountAsync(ct);
			var h = await q.CountAsync(b => b.Pick == MatchPick.Home, ct);
			var d = await q.CountAsync(b => b.Pick == MatchPick.Draw, ct);
			var a = await q.CountAsync(b => b.Pick == MatchPick.Away, ct);

			decimal hp = 0, dp = 0, ap = 0;
			if (total > 0)
			{
				hp = Math.Round(100m * h / total, 2);
				dp = Math.Round(100m * d / total, 2);
				ap = Math.Round(100m * a / total, 2);

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