namespace MatchFixer.Common.DerbyLookup
{
	public static class DerbyLookup
	{
		public static readonly HashSet<(int, int)> DerbyMatches = new()
		{
			(Math.Min(33, 50), Math.Max(33, 50)), // Manchester United vs Manchester City
			(Math.Min(529, 541), Math.Max(529, 541)), // Barcelona vs Real Madrid
		};

		public static bool IsDerby(int teamAId, int teamBId)
		{
			var key = (Math.Min(teamAId, teamBId), Math.Max(teamAId, teamBId));
			return DerbyMatches.Contains(key);
		}
	}

}
