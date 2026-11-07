using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;

using static MatchFixer.Common.GeneralConstants.MarketSelections;

namespace MatchFixer.Core.Helpers
{
	/// <summary>
	/// Settlement and display rules for every betting market, shared by user-facing, admin and settlement code.
	/// </summary>
	public static class BetMarketHelper
	{
		/// <summary>
		/// Resolves a non 1X2 market against the regulation score. Returns Won / Lost.
		/// Penalties are ignored on purpose: goals markets settle on the regulation score.
		/// The caller handles Pending (no result) and Voided (cancelled).
		/// </summary>
		public static BetStatus ResolveMarket(BettingMarket market, string? selection, decimal? line, int home, int away)
		{
			bool won = market switch
			{
				BettingMarket.OverUnderGoals => selection switch
				{
					Over => (home + away) > (line ?? DefaultGoalsLine),
					Under => (home + away) < (line ?? DefaultGoalsLine),
					_ => false
				},

				BettingMarket.BothTeamsToScore => selection switch
				{
					Yes => home > 0 && away > 0,
					No => !(home > 0 && away > 0),
					_ => false
				},

				BettingMarket.DoubleChance => selection switch
				{
					HomeOrDraw => home >= away,
					HomeOrAway => home != away,
					DrawOrAway => away >= home,
					_ => false
				},

				_ => false
			};

			return won ? BetStatus.Won : BetStatus.Lost;
		}

		/// <summary>
		/// True when the bet wins against the given result. Works for every market.
		/// </summary>
		public static bool IsWinningBet(Bet bet, int homeScore, int awayScore, bool? homeWonOnPenalties)
		{
			if (bet.Market != BettingMarket.MatchResult)
				return ResolveMarket(bet.Market, bet.Selection, bet.Line, homeScore, awayScore) == BetStatus.Won;

			var actual = homeWonOnPenalties.HasValue
				? (homeWonOnPenalties.Value ? MatchPick.Home : MatchPick.Away)
				: homeScore > awayScore ? MatchPick.Home
				: homeScore < awayScore ? MatchPick.Away
				: MatchPick.Draw;

			return bet.Pick == actual;
		}

		/// <summary>
		/// Settled status of a bet: Voided when the match is cancelled, Pending without a result, otherwise Won / Lost.
		/// Requires MatchEvent and MatchEvent.LiveResult to be loaded.
		/// </summary>
		public static BetStatus ResolveBetStatus(Bet bet)
		{
			if (bet.MatchEvent.IsCancelled)
				return BetStatus.Voided;

			var result = bet.MatchEvent.LiveResult;
			if (result == null)
				return BetStatus.Pending;

			return IsWinningBet(bet, result.HomeScore, result.AwayScore, result.HomeWonOnPenalties)
				? BetStatus.Won
				: BetStatus.Lost;
		}

		/// <summary>Short label for a pick, e.g. "Home", "Over 2.5", "BTTS Yes", "1X".</summary>
		public static string DisplayPick(Bet bet)
			=> DisplayPick(bet.Market, bet.Pick, bet.Selection, bet.Line);

		public static string DisplayPick(BettingMarket market, MatchPick pick, string? selection, decimal? line)
			=> market switch
			{
				BettingMarket.MatchResult => pick.ToString(),
				BettingMarket.OverUnderGoals => $"{selection} {(line ?? DefaultGoalsLine).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}",
				BettingMarket.BothTeamsToScore => $"BTTS {selection}",
				BettingMarket.DoubleChance => $"DC {selection}",
				_ => selection ?? pick.ToString()
			};

		/// <summary>True when the selection code is valid for the market.</summary>
		public static bool IsValidSelection(BettingMarket market, string? selection)
			=> market switch
			{
				BettingMarket.OverUnderGoals => selection is Over or Under,
				BettingMarket.BothTeamsToScore => selection is Yes or No,
				BettingMarket.DoubleChance => selection is HomeOrDraw or HomeOrAway or DrawOrAway,
				_ => false
			};

		/// <summary>Normalises a selection code coming from the client (case-insensitive) to its canonical form.</summary>
		public static string? NormalizeSelection(BettingMarket market, string? selection)
		{
			if (string.IsNullOrWhiteSpace(selection)) return null;
			var s = selection.Trim();

			string[] options = market switch
			{
				BettingMarket.OverUnderGoals => OverUnder,
				BettingMarket.BothTeamsToScore => BothTeamsToScore,
				BettingMarket.DoubleChance => DoubleChance,
				_ => Array.Empty<string>()
			};

			return options.FirstOrDefault(o => string.Equals(o, s, StringComparison.OrdinalIgnoreCase));
		}
	}
}
