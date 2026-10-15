using System.ComponentModel.DataAnnotations;

using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.Tournaments
{
	// ---------------- List page ----------------

	public class TournamentListViewModel
	{
		public List<TournamentListItemViewModel> Running { get; set; } = new();
		public List<TournamentListItemViewModel> Upcoming { get; set; } = new();
		public List<TournamentListItemViewModel> Finished { get; set; } = new();
		public List<TournamentListItemViewModel> Cancelled { get; set; } = new();
		public List<TournamentLeaderboardRowViewModel> Leaderboard { get; set; } = new();

		public CreateTournamentInputModel CreateForm { get; set; } = new();

		// The viewer's timezone, shown on the create form so they know what the times mean.
		public string UserTimeZoneLabel { get; set; } = "UTC";
	}

	public class TournamentListItemViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }

		public decimal EntryFee { get; set; }
		public decimal StartingBankroll { get; set; }
		public decimal RakePercent { get; set; }

		public TournamentStatus Status { get; set; }
		public string StatusLabel { get; set; }

		public DateTime StartDateUtc { get; set; }
		public DateTime EndDateUtc { get; set; }
		public string StartDisplay { get; set; }
		public string EndDisplay { get; set; }

		public int EntrantsCount { get; set; }
		public int? MaxEntrants { get; set; }

		public decimal PrizePool { get; set; }      // entrants * entry fee
		public decimal NetPrizePool { get; set; }   // prize pool - rake

		public bool IsJoined { get; set; }
		public bool CanJoin { get; set; }

		public string CreatedByName { get; set; }
	}

	public class TournamentLeaderboardRowViewModel
	{
		public int Rank { get; set; }
		public string UserName { get; set; }
		public string Picture { get; set; }
		public decimal TotalPrize { get; set; }
		public int Wins { get; set; }
		public int Podiums { get; set; }
		public int TournamentsPlayed { get; set; }
		public bool IsCurrentUser { get; set; }
	}

	// ---------------- Create form ----------------

	public class CreateTournamentInputModel
	{
		[Required]
		[StringLength(100, MinimumLength = 3)]
		[Display(Name = "Tournament name")]
		public string Name { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		[Range(0, 100000, ErrorMessage = "Entry fee must be 0 or more.")]
		[Display(Name = "Entry fee (€)")]
		public decimal EntryFee { get; set; } = 10m;

		[Required]
		[DataType(DataType.DateTime)]
		[Display(Name = "Starts")]
		public DateTime StartDate { get; set; } = DateTime.UtcNow;

		[Required]
		[DataType(DataType.DateTime)]
		[Display(Name = "Ends")]
		public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);

		[Range(2, 100000)]
		[Display(Name = "Minimum entrants to run")]
		public int MinEntrantsToRun { get; set; } = 2;

		[Range(2, 100000)]
		[Display(Name = "Maximum entrants (optional)")]
		public int? MaxEntrants { get; set; }

		[Range(1, 12)]
		[Display(Name = "Minimum matches per bet")]
		public int MinBetLegs { get; set; } = 1;
	}

	// ---------------- Individual tournament page ----------------

	public class TournamentDetailsViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }

		public TournamentStatus Status { get; set; }
		public string StatusLabel { get; set; }

		public decimal EntryFee { get; set; }
		public decimal StartingBankroll { get; set; }
		public decimal RakePercent { get; set; }

		public DateTime StartDateUtc { get; set; }
		public DateTime EndDateUtc { get; set; }
		public string StartDisplay { get; set; }
		public string EndDisplay { get; set; }

		public int EntrantsCount { get; set; }
		public int? MaxEntrants { get; set; }
		public int MinBetLegs { get; set; } = 1;

		public decimal PrizePool { get; set; }
		public decimal RakeAmount { get; set; }
		public decimal NetPrizePool { get; set; }

		public string CreatedByName { get; set; }
		public bool IsAdmin { get; set; }

		public bool IsJoined { get; set; }
		public bool CanJoin { get; set; }
		public bool CanBet { get; set; }     // joined AND running AND within window

		public MyEntryViewModel? MyEntry { get; set; }

		public List<LeaderboardRowViewModel> Leaderboard { get; set; } = new();
		public List<PrizeBreakdownRow> PrizeBreakdown { get; set; } = new();
		public List<BettableMatchViewModel> BettableMatches { get; set; } = new();
		public List<TournamentBetViewModel> MyBets { get; set; } = new();
		public List<TournamentMessageViewModel> Messages { get; set; } = new();
	}

	public class MyEntryViewModel
	{
		public Guid EntryId { get; set; }
		public decimal Bankroll { get; set; }
		public int Rank { get; set; }
		public int PendingBets { get; set; }
		public int? FinalRank { get; set; }
		public decimal? PrizeAwarded { get; set; }
	}

	public class LeaderboardRowViewModel
	{
		public int Rank { get; set; }
		public string UserName { get; set; }
		public string Picture { get; set; }
		public decimal Bankroll { get; set; }
		public int PendingBets { get; set; }
		public bool IsCurrentUser { get; set; }
		public decimal? PrizeAwarded { get; set; }
		public bool InPrizes { get; set; }
	}

	public class PrizeBreakdownRow
	{
		public int Rank { get; set; }
		public decimal Percent { get; set; }
		public decimal Amount { get; set; }
	}

	public class BettableMatchViewModel
	{
		public Guid MatchId { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public string HomeLogo { get; set; }
		public string AwayLogo { get; set; }
		public decimal? HomeOdds { get; set; }
		public decimal? DrawOdds { get; set; }
		public decimal? AwayOdds { get; set; }
		public bool NoDraw { get; set; }
		public DateTime? KickoffUtc { get; set; }
		public string KickoffDisplay { get; set; }
		public string? CompetitionName { get; set; }
		public string? LeagueName { get; set; }
	}

	public class TournamentBetViewModel
	{
		public Guid SlipId { get; set; }
		public int Legs { get; set; } = 1;
		public string Match { get; set; }
		public string Pick { get; set; }
		public decimal Odds { get; set; }   // combined odds for the slip
		public decimal Stake { get; set; }
		public string Status { get; set; }
		public decimal? WinAmount { get; set; }
		public string PlacedDisplay { get; set; }
		public List<TournamentBetLegViewModel> LegDetails { get; set; } = new();
	}

	public class TournamentBetLegViewModel
	{
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public string HomeLogo { get; set; }
		public string AwayLogo { get; set; }
		public string Pick { get; set; }
		public decimal Odds { get; set; }
		public string Status { get; set; }   // Pending / Won / Lost / Voided
		public string? CompetitionName { get; set; }
		public string? LeagueName { get; set; }
	}

	public class TournamentMessageViewModel
	{
		public string UserName { get; set; }
		public string Picture { get; set; }
		public string Text { get; set; }
		public DateTime PostedAtUtc { get; set; }
		public string PostedDisplay { get; set; }
		public bool IsMine { get; set; }
	}
}
