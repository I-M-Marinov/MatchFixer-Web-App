namespace MatchFixer.Common.Enums
{
	public enum AutoScoreState
	{
		Finished = 0,    // API says the match is over and a score is available -> can be scored
		InProgress = 1,  // still being played
		NotStarted = 2,  // has not kicked off yet
		Abandoned = 3,   // postponed / cancelled / abandoned -> needs void or postpone
		NoData = 4       // API returned nothing for this fixture
	}
}
