namespace MatchFixer.Common.GeneralConstants
{
	public static class WorldCupApiConstants
	{
		// TempData keys
		public const string TempDataRefreshMessage = "RefreshMessage";

		// Admin refresh messages
		public const string MsgKnockoutRefreshed = "Knockout stage refreshed — {0} matches updated.";
		public const string MsgKnockoutNoData = "No knockout matches were updated. The API may not have team data yet.";
		public const string MsgReclassifyRefreshed = "Reclassified and reseeded {0} knockout matches (Round of 32 → Final).";
		public const string MsgReclassifyNoData = "Reclassify complete — no new fixtures returned by the API.";
		public const string MsgStandingsRefreshed = "Group standings refreshed — {0} team rows updated.";
		public const string MsgStandingsNoData = "No standings data returned by the API.";

		// TheSportsDB fixture status 
		public const string StatusMatchFinished = "Match Finished";
		public const string StatusLive = "Live";

		// TheSportsDB strDescription values (standings table) 
		public const string DescriptionRoundOf32 = "Round of 32";

		//Stage display names
		public const string StageNameGroupStage = "Group Stage";
		public const string StageNameRoundOf32 = "Round of 32";
		public const string StageNameRoundOf16 = "Round of 16";
		public const string StageNameQuarterFinal = "Quarter Finals";
		public const string StageNameSemiFinal = "Semi Finals";
		public const string StageNameThirdPlace = "Third Place";
		public const string StageNameFinal = "Final";
	}
}
