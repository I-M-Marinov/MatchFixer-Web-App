using MatchFixer.Common.Enums;

using static MatchFixer.Common.GeneralConstants.MarketSelections;

namespace MatchFixer.Core.Helpers
{
	/// <summary>
	/// Derives prices for the extra markets (Over/Under 2.5, BTTS, Double Chance) from the Match Result (1X2) odds.
	/// Deterministic: the same 1X2 prices always give the same extra-market prices.
	/// </summary>
	public static class MarketOddsGenerator
	{
		public sealed record GeneratedMarketOdds(BettingMarket Market, string Selection, decimal? Line, decimal Odds);

		// Typical football totals: an even game ≈ 2.5 goals, a mismatch pushes it towards ~3.3
		private const double BaseGoalExpectancy = 2.45;
		private const double MismatchGoalBonus = 1.1;

		// House margin: the payout on the "fair" profit part (1/p − 1) is cut by this factor
		private const double PayoutFactor = 0.82;

		private const decimal MinOdds = 1.05m;

		// Double Chance never pays more than this share of the profit of its shorter leg
		private const decimal DoubleChanceCapFactor = 0.6m;
		private const decimal MaxOdds = 8.00m;

		/// <summary>
		/// Returns the extra-market prices, or an empty list when the 1X2 prices are missing.
		/// Double Chance is skipped for No Draw matches.
		/// </summary>
		public static IReadOnlyList<GeneratedMarketOdds> Generate(decimal? homeOdds, decimal? drawOdds, decimal? awayOdds, bool noDraw)
		{
			if (homeOdds is not > 1m || awayOdds is not > 1m)
				return Array.Empty<GeneratedMarketOdds>();

			if (!noDraw && drawOdds is not > 1m)
				return Array.Empty<GeneratedMarketOdds>();

			// Implied probabilities, normalised to remove the book margin
			double ih = 1 / (double)homeOdds.Value;
			double ia = 1 / (double)awayOdds.Value;
			double id = noDraw ? 0 : 1 / (double)drawOdds!.Value;
			double sum = ih + id + ia;

			double pH = ih / sum, pD = id / sum, pA = ia / sum;

			// Goal expectancy from the strength gap, split between the teams by strength
			double gap = Math.Abs(pH - pA);
			double lambda = BaseGoalExpectancy + MismatchGoalBonus * gap;
			double homeShare = Math.Clamp(0.5 + 0.45 * (pH - pA), 0.2, 0.8);
			double lH = lambda * homeShare;
			double lA = lambda * (1 - homeShare);

			// Poisson: P(total ≤ 2) and P(both score)
			double pUnder = Math.Exp(-lambda) * (1 + lambda + lambda * lambda / 2);
			double pOver = 1 - pUnder;
			double pBtts = (1 - Math.Exp(-lH)) * (1 - Math.Exp(-lA));

			var list = new List<GeneratedMarketOdds>
			{
				new(BettingMarket.OverUnderGoals, Over, DefaultGoalsLine, Price(pOver)),
				new(BettingMarket.OverUnderGoals, Under, DefaultGoalsLine, Price(pUnder)),
				new(BettingMarket.BothTeamsToScore, Yes, null, Price(pBtts)),
				new(BettingMarket.BothTeamsToScore, No, null, Price(1 - pBtts)),
			};

			if (!noDraw)
			{
				decimal h = homeOdds.Value, d = drawOdds!.Value, a = awayOdds.Value;

				// A double chance covers two outcomes, so it must always pay less than either of them
				list.Add(new(BettingMarket.DoubleChance, HomeOrDraw, null, CapBelow(Price(pH + pD), h, d)));
				list.Add(new(BettingMarket.DoubleChance, HomeOrAway, null, CapBelow(Price(pH + pA), h, a)));
				list.Add(new(BettingMarket.DoubleChance, DrawOrAway, null, CapBelow(Price(pD + pA), d, a)));
			}

			return list;
		}

		private static decimal CapBelow(decimal price, decimal legA, decimal legB)
		{
			var cap = 1 + (Math.Min(legA, legB) - 1) * DoubleChanceCapFactor;
			return Math.Max(MinOdds, Math.Round(Math.Min(price, cap), 2));
		}

		private static decimal Price(double probability)
		{
			var p = Math.Clamp(probability, 0.01, 0.99);
			var odds = 1 + (1 / p - 1) * PayoutFactor;
			return Math.Clamp(Math.Round((decimal)odds, 2), MinOdds, MaxOdds);
		}
	}
}
