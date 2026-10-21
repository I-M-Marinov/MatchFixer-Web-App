using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Tournaments;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class TournamentService : ITournamentService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IWalletService _wallet;
		private readonly ITimezoneService _timezone;
		private readonly ISessionService _session;

		public TournamentService(
			MatchFixerDbContext db,
			IWalletService wallet,
			ITimezoneService timezone,
			ISessionService session)
		{
			_db = db;
			_wallet = wallet;
			_timezone = timezone;
			_session = session;
		}

		// ---------------- List ----------------

		public async Task<TournamentListViewModel> GetListAsync(Guid currentUserId, bool isAdmin)
		{
			var now = DateTime.UtcNow;
			var tz = _session.GetUserTimezone();

			var tournaments = await _db.Tournaments
				.Include(t => t.CreatedBy)
				.Include(t => t.Entries)
				.AsNoTracking()
				.OrderByDescending(t => t.StartDate)
				.ToListAsync();

			var vm = new TournamentListViewModel { IsAdmin = isAdmin };

			// Pre-fill the create form with the user's LOCAL time (converted back to UTC on save).
			var localNow = _timezone.ConvertToUserTime(now, tz) ?? now;
			vm.CreateForm.StartDate = localNow;
			vm.CreateForm.EndDate = localNow.AddDays(7);
			vm.UserTimeZoneLabel = string.IsNullOrWhiteSpace(tz) ? "UTC" : tz;

			// House-rake ledger: total rake kept on finished, non-Pot tournaments, minus
			// whatever has already been seeded into Pots. Shown to admins as a funding guide.
			decimal totalRake = 0m;
			foreach (var ft in tournaments.Where(x => !x.IsPot && x.Status == TournamentStatus.Finished))
			{
				var ftPool = ft.Entries.Count * ft.EntryFee;
				totalRake += Math.Round(ftPool * ft.RakePercent / 100m, 2, MidpointRounding.AwayFromZero);
			}
			var allocatedToPots = tournaments
				.Where(x => x.IsPot && x.Status != TournamentStatus.Cancelled)
				.Sum(x => x.PotPrizePool ?? 0m);
			vm.TotalRakeCollected = totalRake;
			vm.RakeAllocatedToPots = allocatedToPots;
			vm.AvailableRake = Math.Max(0m, totalRake - allocatedToPots);
			vm.CreatePotForm.PrizePool = vm.AvailableRake > 0m ? Math.Round(vm.AvailableRake, 2) : 100m;

			foreach (var t in tournaments)
			{
				var entrants = t.Entries.Count;
				var pool = entrants * t.EntryFee;
				var rake = Math.Round(pool * t.RakePercent / 100m, 2, MidpointRounding.AwayFromZero);
				var eff = EffectiveStatus(t, now);
				var joined = t.Entries.Any(e => e.UserId == currentUserId);

				var item = new TournamentListItemViewModel
				{
					Id = t.Id,
					Name = t.Name,
					Description = t.Description,
					EntryFee = t.EntryFee,
					StartingBankroll = t.StartingBankroll,
					RakePercent = t.RakePercent,
					Status = eff,
					StatusLabel = StatusLabel(eff),
					StartDateUtc = t.StartDate,
					EndDateUtc = t.EndDate,
					StartDisplay = _timezone.FormatForUserBets(t.StartDate, tz),
					EndDisplay = _timezone.FormatForUserBets(t.EndDate, tz),
					EntrantsCount = entrants,
					MaxEntrants = t.MaxEntrants,
					PrizePool = pool,
					NetPrizePool = pool - rake,
					IsJoined = joined,
					CanJoin = !joined
						&& (eff == TournamentStatus.Upcoming || eff == TournamentStatus.Running)
						&& now < t.EndDate
						&& (t.MaxEntrants == null || entrants < t.MaxEntrants.Value),
					CreatedByName = DisplayName(t.CreatedBy),
					IsPot = t.IsPot,
					PotPrizePool = t.PotPrizePool,
					DurationWeeks = t.DurationWeeks
				};

				// The Pot is admin-funded: its prize pool is the designated amount, not entry fees.
				if (t.IsPot)
				{
					item.PrizePool = t.PotPrizePool ?? 0m;
					item.NetPrizePool = t.PotPrizePool ?? 0m;

					// Cancelled Pots stay private to the people who joined them (and admins).
					if (eff != TournamentStatus.Cancelled || isAdmin || joined)
						vm.Pots.Add(item);

					continue;
				}

				switch (eff)
				{
					case TournamentStatus.Running:
						vm.Running.Add(item);
						break;
					case TournamentStatus.Upcoming:
						vm.Upcoming.Add(item);
						break;
					case TournamentStatus.Cancelled:
						// Cancelled tournaments are private to the people who joined them (and admins).
						if (isAdmin || joined)
							vm.Cancelled.Add(item);
						break;
					default:
						vm.Finished.Add(item);
						break;
				}
			}

			vm.Leaderboard = await BuildTournamentLeaderboardAsync(currentUserId);

			return vm;
		}

		private async Task<List<TournamentLeaderboardRowViewModel>> BuildTournamentLeaderboardAsync(Guid currentUserId)
		{
			var aggs = await _db.TournamentEntries
				.AsNoTracking()
				.Where(e => e.Tournament.Status == TournamentStatus.Finished && e.FinalRank != null)
				.GroupBy(e => e.UserId)
				.Select(g => new
				{
					UserId = g.Key,
					TotalPrize = g.Sum(x => x.PrizeAwarded ?? 0m),
					Wins = g.Count(x => x.FinalRank == 1),
					Podiums = g.Count(x => x.FinalRank <= 3),
					Played = g.Count()
				})
				.ToListAsync();

			if (aggs.Count == 0)
				return new List<TournamentLeaderboardRowViewModel>();

			var userIds = aggs.Select(a => a.UserId).ToList();
			var users = await _db.Users
				.AsNoTracking()
				.Where(u => userIds.Contains(u.Id))
				.Include(u => u.ProfilePicture)
				.ToDictionaryAsync(u => u.Id);

			var ordered = aggs
				.OrderByDescending(a => a.TotalPrize)
				.ThenByDescending(a => a.Wins)
				.ThenByDescending(a => a.Podiums)
				.ThenByDescending(a => a.Played)
				.Take(100)
				.ToList();

			var rows = new List<TournamentLeaderboardRowViewModel>(ordered.Count);
			for (int i = 0; i < ordered.Count; i++)
			{
				var a = ordered[i];
				users.TryGetValue(a.UserId, out var u);
				rows.Add(new TournamentLeaderboardRowViewModel
				{
					Rank = i + 1,
					UserName = DisplayName(u),
					Picture = AvatarUrl(u),
					TotalPrize = a.TotalPrize,
					Wins = a.Wins,
					Podiums = a.Podiums,
					TournamentsPlayed = a.Played,
					IsCurrentUser = a.UserId == currentUserId
				});
			}
			return rows;
		}

		// ---------------- Details ----------------

		private static string DisplayName(ApplicationUser? user)
		{
			if (user == null)
				return "Player";

			var full = user.FullName?.Trim();
			if (!string.IsNullOrWhiteSpace(full))
				return full;

			var email = user.UserName ?? user.Email;
			if (!string.IsNullOrWhiteSpace(email))
			{
				var at = email.IndexOf('@');
				return at > 0 ? email.Substring(0, at) : email;
			}

			return "Player";
		}

		private static string AvatarUrl(ApplicationUser? user)
		{
			var url = user?.ProfilePicture?.ImageUrl;
			return string.IsNullOrWhiteSpace(url)
				? ProfilePictureConstants.DefaultImagePath
				: url;
		}

		public async Task<TournamentDetailsViewModel?> GetDetailsAsync(Guid tournamentId, Guid currentUserId, bool isAdmin)
		{
			var now = DateTime.UtcNow;
			var tz = _session.GetUserTimezone();

			var t = await _db.Tournaments
				.Include(x => x.CreatedBy)
				.Include(x => x.Entries).ThenInclude(e => e.User).ThenInclude(u => u.ProfilePicture)
				.Include(x => x.Messages).ThenInclude(m => m.User).ThenInclude(u => u.ProfilePicture)
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == tournamentId);

			if (t == null)
				return null;

			var entries = t.Entries.ToList();

			// Cancelled tournaments are private: only participants (and admins) may open them.
			if (t.Status == TournamentStatus.Cancelled
				&& !isAdmin
				&& !entries.Any(e => e.UserId == currentUserId))
			{
				return null;
			}
			var entrants = entries.Count;

			// The viewer may see bankrolls and the board ONLY if they are in the tournament
			// (or an admin). Non-participants get leaderboard POSITIONS and nothing else.
			var joinedThis = entries.Any(e => e.UserId == currentUserId);
			var canSeePrivate = isAdmin || joinedThis;

			decimal pool, rake, net;
			if (t.IsPot)
			{
				// Admin-funded: the pool is the designated prize, no rake, no entry fees.
				pool = t.PotPrizePool ?? 0m;
				rake = 0m;
				net = pool;
			}
			else
			{
				pool = entrants * t.EntryFee;
				rake = Math.Round(pool * t.RakePercent / 100m, 2, MidpointRounding.AwayFromZero);
				net = pool - rake;
			}

			var entryIds = entries.Select(e => e.Id).ToList();

			var slips = await _db.BetSlips
				.Include(s => s.Bets).ThenInclude(b => b.MatchEvent).ThenInclude(me => me.HomeTeam)
				.Include(s => s.Bets).ThenInclude(b => b.MatchEvent).ThenInclude(me => me.AwayTeam)
				.Where(s => s.TournamentEntryId != null && entryIds.Contains(s.TournamentEntryId.Value))
				.AsNoTracking()
				.ToListAsync();

			var pendingByEntry = slips
				.Where(s => !s.IsSettled)
				.GroupBy(s => s.TournamentEntryId!.Value)
				.ToDictionary(g => g.Key, g => g.Count());

			// Rank by bankroll desc, earliest join breaks ties.
			var ranked = entries
				.OrderByDescending(e => e.Bankroll)
				.ThenBy(e => e.JoinedAtUtc)
				.ToList();

			var breakdown = t.IsPot
				? BuildPotBreakdown(net, entrants, t.PotFirstPct, t.PotSecondPct, t.PotThirdPct)
				: BuildPrizeBreakdown(net, entrants);
			var paidRanks = breakdown.Select(b => b.Rank).ToHashSet();

			var leaderboard = new List<LeaderboardRowViewModel>();
			for (int i = 0; i < ranked.Count; i++)
			{
				var e = ranked[i];
				var rank = i + 1;
				leaderboard.Add(new LeaderboardRowViewModel
				{
					Rank = rank,
					UserName = DisplayName(e.User),
					Picture = AvatarUrl(e.User),
					// Hide other players' bankrolls / in-play counts from non-participants.
					Bankroll = canSeePrivate ? e.Bankroll : 0m,
					PendingBets = canSeePrivate ? (pendingByEntry.TryGetValue(e.Id, out var pc) ? pc : 0) : 0,
					IsCurrentUser = e.UserId == currentUserId,
					PrizeAwarded = e.PrizeAwarded,
					InPrizes = paidRanks.Contains(rank)
				});
			}

			var myEntry = entries.FirstOrDefault(e => e.UserId == currentUserId);
			MyEntryViewModel? myVm = null;
			if (myEntry != null)
			{
				var myRank = leaderboard.First(r => r.IsCurrentUser).Rank;
				myVm = new MyEntryViewModel
				{
					EntryId = myEntry.Id,
					Bankroll = myEntry.Bankroll,
					Rank = myRank,
					PendingBets = pendingByEntry.TryGetValue(myEntry.Id, out var mp) ? mp : 0,
					FinalRank = myEntry.FinalRank,
					PrizeAwarded = myEntry.PrizeAwarded
				};
			}

			var eff = EffectiveStatus(t, now);
			var joined = myEntry != null;
			var canJoin = !joined
				&& (eff == TournamentStatus.Upcoming || eff == TournamentStatus.Running)
				&& now < t.EndDate
				&& (t.MaxEntrants == null || entrants < t.MaxEntrants.Value);
			var canBet = joined
				&& eff == TournamentStatus.Running
				&& now >= t.StartDate && now < t.EndDate;

			// My bets (single or multi-leg parlay slips).
			var myBets = new List<TournamentBetViewModel>();
			if (myEntry != null)
			{
				foreach (var s in slips.Where(s => s.TournamentEntryId == myEntry.Id).OrderByDescending(s => s.BetTime))
				{
					var legBets = s.Bets.ToList();
					var legCount = legBets.Count;
					var combinedOdds = legBets.Aggregate(1m, (acc, b) => acc * b.Odds);

					var legDetails = legBets.Select(b => new TournamentBetLegViewModel
					{
						HomeTeam = b.MatchEvent?.HomeTeam?.Name ?? "Home",
						AwayTeam = b.MatchEvent?.AwayTeam?.Name ?? "Away",
						HomeLogo = b.MatchEvent?.HomeTeam == null
							? ""
							: (string.IsNullOrWhiteSpace(b.MatchEvent.HomeTeam.LocalLogoUrl) ? b.MatchEvent.HomeTeam.LogoUrl : b.MatchEvent.HomeTeam.LocalLogoUrl),
						AwayLogo = b.MatchEvent?.AwayTeam == null
							? ""
							: (string.IsNullOrWhiteSpace(b.MatchEvent.AwayTeam.LocalLogoUrl) ? b.MatchEvent.AwayTeam.LogoUrl : b.MatchEvent.AwayTeam.LocalLogoUrl),
						Pick = b.Pick.ToString(),
						Odds = b.Odds,
						Status = b.Status.ToString(),
						CompetitionName = b.MatchEvent?.CompetitionName,
						LeagueName = b.MatchEvent?.HomeTeam?.LeagueName
					}).ToList();

					var firstBet = legBets.FirstOrDefault();
					var firstMe = firstBet?.MatchEvent;

					myBets.Add(new TournamentBetViewModel
					{
						SlipId = s.Id,
						Legs = legCount,
						Match = legCount <= 1
							? (firstMe != null ? $"{firstMe.HomeTeam?.Name} vs {firstMe.AwayTeam?.Name}" : "-")
							: $"{legCount}-leg parlay",
						Pick = legCount <= 1 ? (firstBet?.Pick.ToString() ?? "-") : "",
						Odds = combinedOdds,
						Stake = s.Amount,
						Status = SlipStatusLabel(s),
						WinAmount = s.WinAmount,
						PlacedDisplay = _timezone.FormatForUserBets(s.BetTime, tz),
						LegDetails = legDetails
					});
				}
			}

			// Matches that can be bet on right now.
			var bettable = new List<BettableMatchViewModel>();
			if (canBet)
			{
				var matches = await _db.MatchEvents
					.Include(m => m.HomeTeam)
					.Include(m => m.AwayTeam)
					.Include(m => m.LiveResult)
					.Where(m => !m.IsCancelled && !m.IsPostponed)
					.Where(m => m.LiveResult == null)
					.Where(m => m.Status != MatchStatus.FullTime)
					.Where(m => m.MatchDate != null && m.MatchDate > now && m.MatchDate < t.EndDate)
					.OrderBy(m => m.MatchDate)
					.Take(60)
					.AsNoTracking()
					.ToListAsync();

				foreach (var m in matches)
				{
					bettable.Add(new BettableMatchViewModel
					{
						MatchId = m.Id,
						HomeTeam = m.HomeTeam.Name,
						AwayTeam = m.AwayTeam.Name,
						HomeLogo = string.IsNullOrWhiteSpace(m.HomeTeam.LocalLogoUrl) ? m.HomeTeam.LogoUrl : m.HomeTeam.LocalLogoUrl,
						AwayLogo = string.IsNullOrWhiteSpace(m.AwayTeam.LocalLogoUrl) ? m.AwayTeam.LogoUrl : m.AwayTeam.LocalLogoUrl,
						HomeOdds = m.HomeOdds,
						DrawOdds = m.DrawOdds,
						AwayOdds = m.AwayOdds,
						NoDraw = m.NoDraw,
						KickoffUtc = m.MatchDate,
						KickoffDisplay = _timezone.FormatForUserBets(m.MatchDate, tz),
						CompetitionName = m.CompetitionName,
						LeagueName = m.HomeTeam?.LeagueName
					});
				}
			}

			// The board is private to participants (and admins); non-participants get nothing.
			var messages = canSeePrivate
				? t.Messages
					.Where(mm => !mm.IsDeleted)
					.OrderBy(mm => mm.PostedAtUtc)
					.Select(mm => new TournamentMessageViewModel
					{
						UserName = DisplayName(mm.User),
						Picture = AvatarUrl(mm.User),
						Text = mm.Text,
						PostedAtUtc = mm.PostedAtUtc,
						PostedDisplay = _timezone.FormatForUserBets(mm.PostedAtUtc, tz),
						IsMine = mm.UserId == currentUserId
					})
					.ToList()
				: new List<TournamentMessageViewModel>();

			return new TournamentDetailsViewModel
			{
				Id = t.Id,
				Name = t.Name,
				Description = t.Description,
				Status = eff,
				StatusLabel = StatusLabel(eff),
				EntryFee = t.EntryFee,
				StartingBankroll = t.StartingBankroll,
				RakePercent = t.RakePercent,
				StartDateUtc = t.StartDate,
				EndDateUtc = t.EndDate,
				StartDisplay = _timezone.FormatForUserBets(t.StartDate, tz),
				EndDisplay = _timezone.FormatForUserBets(t.EndDate, tz),
				EntrantsCount = entrants,
				MaxEntrants = t.MaxEntrants,
				MinBetLegs = t.MinBetLegs,
				PrizePool = pool,
				RakeAmount = rake,
				NetPrizePool = net,
				CreatedByName = DisplayName(t.CreatedBy),
				IsAdmin = isAdmin,
				IsJoined = joined,
				CanJoin = canJoin,
				CanBet = canBet,
				IsPot = t.IsPot,
				PotPrizePool = t.PotPrizePool,
				DurationWeeks = t.DurationWeeks,
				CanSeeBankrolls = canSeePrivate,
				CanSeeBoard = canSeePrivate,
				MyEntry = myVm,
				Leaderboard = leaderboard,
				PrizeBreakdown = breakdown,
				BettableMatches = bettable,
				MyBets = myBets,
				Messages = messages
			};
		}

		// ---------------- Create ----------------

		public async Task<(bool Success, string Message, Guid? TournamentId)> CreateAsync(Guid currentUserId, CreateTournamentInputModel input, bool isAdmin)
		{
			var now = DateTime.UtcNow;

			if (input == null || string.IsNullOrWhiteSpace(input.Name))
				return (false, "Name is required.", null);

			// The datetime-local inputs are the user's LOCAL wall-clock time; convert to UTC for storage.
			var tz = _session.GetUserTimezone();
			DateTime start, end;
			if (string.IsNullOrWhiteSpace(tz))
			{
				start = DateTime.SpecifyKind(input.StartDate, DateTimeKind.Utc);
				end = DateTime.SpecifyKind(input.EndDate, DateTimeKind.Utc);
			}
			else
			{
				start = _timezone.ConvertFromUserTimeToUtc(input.StartDate, tz);
				end = _timezone.ConvertFromUserTimeToUtc(input.EndDate, tz);
			}

			if (end <= start)
				return (false, "End time must be after the start time.", null);
			if (end <= now)
				return (false, "End time must be in the future.", null);
			if (input.EntryFee < 0)
				return (false, "Entry fee cannot be negative.", null);

			var minEntrants = input.MinEntrantsToRun < 2 ? 2 : input.MinEntrantsToRun;
			if (input.MaxEntrants.HasValue && input.MaxEntrants.Value < minEntrants)
				return (false, "Maximum entrants cannot be less than the minimum.", null);

			var minBetLegs = input.MinBetLegs < 1 ? 1 : (input.MinBetLegs > 12 ? 12 : input.MinBetLegs);

			// User-created tournaments go live immediately: never start in the past.
			if (start < now)
				start = now;

			var t = new Tournament
			{
				Id = Guid.NewGuid(),
				Name = input.Name.Trim(),
				Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
				EntryFee = input.EntryFee,
				StartingBankroll = 1000m,
				RakePercent = 10m,
				StartDate = start,
				EndDate = end,
				Status = TournamentStatus.Upcoming,
				MinEntrantsToRun = minEntrants,
				MaxEntrants = input.MaxEntrants,
				MinBetLegs = minBetLegs,
				CreatedByUserId = currentUserId,
				CreatedAtUtc = now
			};

			_db.Tournaments.Add(t);
			await _db.SaveChangesAsync();

			// The creator is entered automatically (charged the entry fee, given a bankroll).
			var (joined, joinMsg) = await JoinAsync(t.Id, currentUserId);
			var resultMsg = joined
				? $"Tournament created — you're in with {t.StartingBankroll:0} chips!"
				: $"Tournament created, but you weren't entered: {joinMsg}";

			return (true, resultMsg, t.Id);
		}

		// ---------------- Create The Pot (admin) ----------------

		public async Task<(bool Success, string Message, Guid? TournamentId)> CreatePotAsync(Guid currentUserId, CreatePotInputModel input)
		{
			var now = DateTime.UtcNow;

			if (input == null || string.IsNullOrWhiteSpace(input.Name))
				return (false, "Pot name is required.", null);
			if (input.PrizePool <= 0m)
				return (false, "The prize pool must be greater than zero.", null);

			// Pots run for 2, 4 or 6 weeks.
			var weeks = input.DurationWeeks;
			if (weeks != 2 && weeks != 4 && weeks != 6)
				weeks = 2;

			var first = input.FirstPct < 0 ? 0m : input.FirstPct;
			var second = input.SecondPct < 0 ? 0m : input.SecondPct;
			var third = input.ThirdPct < 0 ? 0m : input.ThirdPct;
			if (first + second + third <= 0m)
			{
				first = 60m; second = 30m; third = 10m;
			}

			var minBetLegs = input.MinBetLegs < 1 ? 1 : (input.MinBetLegs > 12 ? 12 : input.MinBetLegs);
			var startingBankroll = input.StartingBankroll <= 0m ? 1000m : input.StartingBankroll;
			var minEntrants = input.MinEntrantsToRun < 1 ? 1 : input.MinEntrantsToRun;

			var t = new Tournament
			{
				Id = Guid.NewGuid(),
				Name = input.Name.Trim(),
				Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
				EntryFee = 0m,                 // free to enter
				StartingBankroll = startingBankroll,
				RakePercent = 0m,              // admin-funded; no rake taken
				StartDate = now,               // opens immediately
				EndDate = now.AddDays(weeks * 7),
				Status = TournamentStatus.Upcoming,
				MinEntrantsToRun = minEntrants,
				MaxEntrants = null,
				MinBetLegs = minBetLegs,
				CreatedByUserId = currentUserId,
				CreatedAtUtc = now,
				IsPot = true,
				PotPrizePool = Math.Round(input.PrizePool, 2, MidpointRounding.AwayFromZero),
				PotFirstPct = first,
				PotSecondPct = second,
				PotThirdPct = third,
				DurationWeeks = weeks
			};

			_db.Tournaments.Add(t);
			await _db.SaveChangesAsync();

			return (true, $"The Pot \"{t.Name}\" is live — €{t.PotPrizePool:0.##} prize, running {weeks} weeks.", t.Id);
		}

		// ---------------- Join ----------------

		public async Task<(bool Success, string Message)> JoinAsync(Guid tournamentId, Guid currentUserId)
		{
			var now = DateTime.UtcNow;

			var t = await _db.Tournaments
				.Include(x => x.Entries)
				.FirstOrDefaultAsync(x => x.Id == tournamentId);

			if (t == null)
				return (false, "Tournament not found.");
			if (t.Status == TournamentStatus.Finished || t.Status == TournamentStatus.Cancelled || now >= t.EndDate)
				return (false, "This tournament has ended.");
			if (t.Entries.Any(e => e.UserId == currentUserId))
				return (false, "You have already joined this tournament.");
			if (t.MaxEntrants.HasValue && t.Entries.Count >= t.MaxEntrants.Value)
				return (false, "This tournament is full.");

			// Charge the real-money entry fee.
			var (ok, msg) = await _wallet.ChargeTournamentEntryAsync(currentUserId, t.EntryFee, t.Id);
			if (!ok)
				return (false, msg);

			var entry = new TournamentEntry
			{
				Id = Guid.NewGuid(),
				TournamentId = t.Id,
				UserId = currentUserId,
				Bankroll = t.StartingBankroll,
				EntryFeePaid = t.EntryFee,
				JoinedAtUtc = now
			};

			try
			{
				_db.TournamentEntries.Add(entry);
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				// Lost a race on the unique (TournamentId, UserId) index - give the money back.
				await _wallet.RefundTournamentEntryAsync(currentUserId, t.EntryFee, t.Id);
				return (false, "Could not join (you may already be entered).");
			}

			return (true, $"You're in! You have {t.StartingBankroll:0} chips to play with.");
		}

		// ---------------- Place a bankroll bet ----------------

		public async Task<(bool Success, string Message)> PlaceBetAsync(Guid tournamentId, Guid currentUserId, IReadOnlyList<(Guid MatchId, string Pick)> legs, decimal stake)
		{
			static string Label(MatchEvent m) => $"{m.HomeTeam?.Name ?? "Match"} vs {m.AwayTeam?.Name ?? ""}".Trim();

			var now = DateTime.UtcNow;

			if (legs == null || legs.Count == 0)
				return (false, "Pick at least one match to bet on.");
			if (stake < 1m)
				return (false, "Minimum stake is 1 chip.");

			var t = await _db.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId);
			if (t == null)
				return (false, "Tournament not found.");

			var eff = EffectiveStatus(t, now);
			if (eff != TournamentStatus.Running || now < t.StartDate || now >= t.EndDate)
				return (false, "Betting is closed for this tournament.");

			// One pick per match.
			var distinctMatchIds = legs.Select(l => l.MatchId).Distinct().ToList();
			if (distinctMatchIds.Count != legs.Count)
				return (false, "You can only pick one outcome per match.");

			if (legs.Count < t.MinBetLegs)
				return (false, t.MinBetLegs <= 1
					? "Pick at least one match."
					: $"This tournament requires a parlay of at least {t.MinBetLegs} matches.");
			if (legs.Count > 12)
				return (false, "A bet can have at most 12 legs.");

			var entry = await _db.TournamentEntries
				.FirstOrDefaultAsync(e => e.TournamentId == tournamentId && e.UserId == currentUserId);
			if (entry == null)
				return (false, "Join the tournament before placing a bet.");

			if (stake > entry.Bankroll)
				return (false, "Not enough chips in your bankroll.");

			var matches = await _db.MatchEvents
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Where(m => distinctMatchIds.Contains(m.Id))
				.ToListAsync();

			var slip = new BetSlip
			{
				Id = Guid.NewGuid(),
				UserId = currentUserId,
				TournamentEntryId = entry.Id,
				Amount = stake,
				BetTime = now,
				IsSettled = false
			};

			decimal combinedOdds = 1m;

			foreach (var leg in legs)
			{
				var match = matches.FirstOrDefault(m => m.Id == leg.MatchId);
				if (match == null)
					return (false, "One of the picked matches was not found.");
				if (match.IsCancelled)
					return (false, $"{Label(match)} is cancelled.");
				if (match.IsPostponed)
					return (false, $"{Label(match)} is postponed.");
				if (match.LiveResult != null || match.Status == MatchStatus.FullTime)
					return (false, $"{Label(match)} is already finished.");
				if (match.MatchDate == null || match.MatchDate <= now)
					return (false, $"{Label(match)} has already started.");
				if (match.MatchDate >= t.EndDate)
					return (false, $"{Label(match)} finishes after the tournament ends.");

				if (!Enum.TryParse<MatchPick>(leg.Pick, true, out var parsedPick))
					return (false, "Invalid pick.");
				if (parsedPick == MatchPick.Draw && match.NoDraw)
					return (false, $"Draw is not available for {Label(match)}.");

				decimal? odds = parsedPick switch
				{
					MatchPick.Home => match.HomeOdds,
					MatchPick.Draw => match.DrawOdds,
					MatchPick.Away => match.AwayOdds,
					_ => null
				};
				if (odds == null || odds <= 0m)
					return (false, $"Odds are not available for your pick on {Label(match)}.");

				combinedOdds *= odds.Value;

				slip.Bets.Add(new Bet
				{
					Id = Guid.NewGuid(),
					BetSlipId = slip.Id,
					MatchEventId = match.Id,
					Pick = parsedPick,
					Odds = odds.Value,
					BetTime = now,
					Status = BetStatus.Pending
				});
			}

			entry.Bankroll -= stake;

			_db.BetSlips.Add(slip);
			await _db.SaveChangesAsync();

			return (true, legs.Count == 1
				? $"Bet placed: {stake:0.##} chips @ {combinedOdds:0.00}."
				: $"{legs.Count}-leg parlay placed: {stake:0.##} chips @ {combinedOdds:0.00} (potential {Math.Round(stake * combinedOdds, 2):0.##}).");
		}

		// ---------------- Board ----------------

		public async Task<(bool Success, string Message)> PostMessageAsync(Guid tournamentId, Guid currentUserId, string text)
		{
			var body = (text ?? string.Empty).Trim();
			if (body.Length == 0)
				return (false, "Message is empty.");
			if (body.Length > 1000)
				body = body.Substring(0, 1000);

			var exists = await _db.Tournaments.AnyAsync(x => x.Id == tournamentId);
			if (!exists)
				return (false, "Tournament not found.");

			var joined = await _db.TournamentEntries
				.AnyAsync(e => e.TournamentId == tournamentId && e.UserId == currentUserId);
			if (!joined)
				return (false, "Only entrants can post on the board.");

			_db.TournamentMessages.Add(new TournamentMessage
			{
				Id = Guid.NewGuid(),
				TournamentId = tournamentId,
				UserId = currentUserId,
				Text = body,
				PostedAtUtc = DateTime.UtcNow
			});
			await _db.SaveChangesAsync();

			return (true, "Posted.");
		}

		// ---------------- Admin / lifecycle ----------------

		public async Task<(bool Success, string Message)> CancelAsync(Guid tournamentId)
		{
			var t = await _db.Tournaments
				.Include(x => x.Entries)
				.FirstOrDefaultAsync(x => x.Id == tournamentId);

			if (t == null)
				return (false, "Tournament not found.");
			if (t.Status == TournamentStatus.Finished)
				return (false, "Tournament already finished.");
			if (t.Status == TournamentStatus.Cancelled)
				return (false, "Tournament already cancelled.");

			await VoidUnsettledBetsAsync(t);

			if (!t.PrizePoolSettled)
			{
				foreach (var e in t.Entries)
				{
					if (e.EntryFeePaid > 0 &&
						!await _wallet.HasTournamentTransactionAsync(e.UserId, WalletTransactionType.TournamentRefund, t.Id))
					{
						await _wallet.RefundTournamentEntryAsync(e.UserId, e.EntryFeePaid, t.Id);
					}
				}
			}

			t.Status = TournamentStatus.Cancelled;
			t.PrizePoolSettled = true;
			t.SettledAtUtc = DateTime.UtcNow;
			await _db.SaveChangesAsync();

			return (true, "Tournament cancelled and entry fees refunded.");
		}

		public async Task<(bool Success, string Message)> SettleAsync(Guid tournamentId)
		{
			var now = DateTime.UtcNow;

			var t = await _db.Tournaments
				.Include(x => x.Entries)
				.FirstOrDefaultAsync(x => x.Id == tournamentId);

			if (t == null)
				return (false, "Tournament not found.");
			if (t.PrizePoolSettled || t.Status == TournamentStatus.Finished || t.Status == TournamentStatus.Cancelled)
				return (false, "Tournament already settled.");

			// 1) Void any still-pending tournament bets so chips return and the table is final.
			await VoidUnsettledBetsAsync(t);

			var entrants = t.Entries.Count;

			// 2) Not enough entrants -> cancel + refund fees.
			if (entrants < t.MinEntrantsToRun)
			{
				foreach (var e in t.Entries)
				{
					if (e.EntryFeePaid > 0 &&
						!await _wallet.HasTournamentTransactionAsync(e.UserId, WalletTransactionType.TournamentRefund, t.Id))
					{
						await _wallet.RefundTournamentEntryAsync(e.UserId, e.EntryFeePaid, t.Id);
					}
				}

				t.Status = TournamentStatus.Cancelled;
				t.PrizePoolSettled = true;
				t.SettledAtUtc = now;
				await _db.SaveChangesAsync();
				return (true, t.IsPot
					? "The Pot was cancelled — not enough players joined."
					: "Tournament cancelled (not enough entrants) - entry fees refunded.");
			}

			// 3) Rank and pay out the net pool.
			decimal net;
			List<PrizeBreakdownRow> breakdown;
			if (t.IsPot)
			{
				// Admin-funded prize, no rake / entry fees.
				net = t.PotPrizePool ?? 0m;
				breakdown = BuildPotBreakdown(net, entrants, t.PotFirstPct, t.PotSecondPct, t.PotThirdPct);
			}
			else
			{
				var pool = entrants * t.EntryFee;
				var rake = Math.Round(pool * t.RakePercent / 100m, 2, MidpointRounding.AwayFromZero);
				net = pool - rake;
				breakdown = BuildPrizeBreakdown(net, entrants);
			}

			var ranked = t.Entries
				.OrderByDescending(e => e.Bankroll)
				.ThenBy(e => e.JoinedAtUtc)
				.ToList();

			for (int i = 0; i < ranked.Count; i++)
			{
				var e = ranked[i];
				var rank = i + 1;
				e.FinalRank = rank;

				var prizeRow = breakdown.FirstOrDefault(b => b.Rank == rank);
				if (prizeRow != null && prizeRow.Amount > 0)
				{
					e.PrizeAwarded = prizeRow.Amount;
					if (!await _wallet.HasTournamentTransactionAsync(e.UserId, WalletTransactionType.TournamentPrize, t.Id))
					{
						await _wallet.AwardTournamentPrizeAsync(e.UserId, prizeRow.Amount, t.Id, rank);
					}
				}
				else
				{
					e.PrizeAwarded = 0m;
				}
			}

			t.Status = TournamentStatus.Finished;
			t.PrizePoolSettled = true;
			t.SettledAtUtc = now;
			await _db.SaveChangesAsync();

			return (true, t.IsPot
				? "The Pot is settled — prizes paid to the top finishers."
				: "Tournament settled and prizes paid.");
		}

		public async Task RunLifecycleAsync(CancellationToken ct)
		{
			var now = DateTime.UtcNow;

			// Flip Upcoming -> Running once their start time has passed.
			var toStart = await _db.Tournaments
				.Where(t => t.Status == TournamentStatus.Upcoming && t.StartDate <= now && t.EndDate > now)
				.ToListAsync(ct);

			foreach (var t in toStart)
				t.Status = TournamentStatus.Running;

			if (toStart.Count > 0)
				await _db.SaveChangesAsync(ct);

			// Settle anything past its end that is not settled yet.
			var toSettle = await _db.Tournaments
				.Where(t => !t.PrizePoolSettled
						 && t.Status != TournamentStatus.Finished
						 && t.Status != TournamentStatus.Cancelled
						 && t.EndDate <= now)
				.Select(t => t.Id)
				.ToListAsync(ct);

			foreach (var id in toSettle)
			{
				if (ct.IsCancellationRequested)
					break;

				try
				{
					await SettleAsync(id);
				}
				catch
				{
					// One tournament failing must not stop the rest.
				}
			}
		}

		// ---------------- Helpers ----------------

		private async Task VoidUnsettledBetsAsync(Tournament t)
		{
			var entryIds = t.Entries.Select(e => e.Id).ToList();
			if (entryIds.Count == 0)
				return;

			var unsettled = await _db.BetSlips
				.Include(s => s.Bets)
				.Where(s => s.TournamentEntryId != null
						 && entryIds.Contains(s.TournamentEntryId.Value)
						 && !s.IsSettled)
				.ToListAsync();

			if (unsettled.Count == 0)
				return;

			var entryById = t.Entries.ToDictionary(e => e.Id);

			foreach (var s in unsettled)
			{
				foreach (var b in s.Bets)
					b.Status = BetStatus.Voided;

				s.IsSettled = true;
				s.SettledAt = DateTime.UtcNow;
				s.WinAmount = 0m;

				if (s.TournamentEntryId.HasValue && entryById.TryGetValue(s.TournamentEntryId.Value, out var e))
					e.Bankroll += s.Amount; // return the stake to the bankroll
			}

			await _db.SaveChangesAsync();
		}

		private static TournamentStatus EffectiveStatus(Tournament t, DateTime now)
		{
			if (t.Status == TournamentStatus.Finished || t.Status == TournamentStatus.Cancelled)
				return t.Status;
			if (now < t.StartDate)
				return TournamentStatus.Upcoming;
			return TournamentStatus.Running; // started and not yet settled
		}

		private static string StatusLabel(TournamentStatus s) => s switch
		{
			TournamentStatus.Upcoming => "Upcoming",
			TournamentStatus.Running => "Running",
			TournamentStatus.Finished => "Finished",
			TournamentStatus.Cancelled => "Cancelled",
			_ => s.ToString()
		};

		private static string SlipStatusLabel(BetSlip s)
		{
			if (!s.IsSettled)
				return "Pending";
			if (s.Bets.Any(b => b.Status == BetStatus.Voided))
				return "Voided";
			return (s.WinAmount ?? 0m) > 0m ? "Won" : "Lost";
		}

		// 4+ entrants -> 60/30/10, 2-3 -> 70/30, otherwise no payout (cancel+refund).
		private static List<(int Rank, decimal Pct)> PayoutTiers(int entrants)
		{
			if (entrants >= 4)
				return new List<(int, decimal)> { (1, 0.60m), (2, 0.30m), (3, 0.10m) };
			if (entrants >= 2)
				return new List<(int, decimal)> { (1, 0.70m), (2, 0.30m) };
			return new List<(int, decimal)>();
		}

		private static List<PrizeBreakdownRow> BuildPrizeBreakdown(decimal netPool, int entrants)
		{
			var tiers = PayoutTiers(entrants);
			var rows = new List<PrizeBreakdownRow>();
			decimal allocated = 0m;

			foreach (var tier in tiers)
			{
				var amt = Math.Round(netPool * tier.Pct, 2, MidpointRounding.AwayFromZero);
				rows.Add(new PrizeBreakdownRow { Rank = tier.Rank, Percent = tier.Pct * 100m, Amount = amt });
				allocated += amt;
			}

			// Any rounding remainder goes to 1st place so the shares sum to the net pool exactly.
			if (rows.Count > 0)
			{
				var remainder = netPool - allocated;
				if (remainder != 0m)
					rows[0].Amount += remainder;
			}

			return rows;
		}

		// The Pot pays its top 3 by an admin-set split. If fewer than 3 players joined,
		// the present ranks' shares are normalised so the whole prize is still paid out.
		private static List<PrizeBreakdownRow> BuildPotBreakdown(decimal netPool, int entrants, decimal? p1, decimal? p2, decimal? p3)
		{
			var raw = new List<(int Rank, decimal Pct)>();
			if (entrants >= 1 && (p1 ?? 0m) > 0m) raw.Add((1, (p1 ?? 0m) / 100m));
			if (entrants >= 2 && (p2 ?? 0m) > 0m) raw.Add((2, (p2 ?? 0m) / 100m));
			if (entrants >= 3 && (p3 ?? 0m) > 0m) raw.Add((3, (p3 ?? 0m) / 100m));

			var rows = new List<PrizeBreakdownRow>();
			if (raw.Count == 0 || netPool <= 0m)
				return rows;

			var totalPct = raw.Sum(x => x.Pct);
			decimal allocated = 0m;

			foreach (var tier in raw)
			{
				var share = totalPct > 0m ? tier.Pct / totalPct : 0m;
				var amt = Math.Round(netPool * share, 2, MidpointRounding.AwayFromZero);
				rows.Add(new PrizeBreakdownRow
				{
					Rank = tier.Rank,
					Percent = Math.Round(share * 100m, 2, MidpointRounding.AwayFromZero),
					Amount = amt
				});
				allocated += amt;
			}

			// Rounding remainder to 1st so the shares sum to the whole prize.
			var remainder = netPool - allocated;
			if (remainder != 0m)
				rows[0].Amount += remainder;

			return rows;
		}
	}
}
