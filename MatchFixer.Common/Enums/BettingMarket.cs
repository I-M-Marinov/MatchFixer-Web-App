namespace MatchFixer.Common.Enums
{
	public enum BettingMarket
	{
		MatchResult = 0,      // Home / Draw / Away (odds live on the MatchEvent columns)
		OverUnderGoals = 1,   // selection: "Over" / "Under", Line = 2.5
		BothTeamsToScore = 2, // selection: "Yes" / "No"
		DoubleChance = 3      // selection: "1X" / "12" / "X2"
	}
}
