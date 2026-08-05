using MatchFixer.Common.VirtualLeagues;

namespace MatchFixer.Common.FootballLeagues
{
	/// <summary>
	/// Canonical league names as stored on Team.LeagueName — shared across all projects.
	/// </summary>
	public static class LeagueNameMap
	{
		/// <summary>
		/// Domestic leagues only (excludes virtual / international leagues).
		/// Keys match FootballApiConstants league IDs; values match Team.LeagueName in the DB.
		/// </summary>
		public static readonly IReadOnlyDictionary<int, string> Domestic = new Dictionary<int, string>
		{
			{ 39,  "Premier League" },
			{ 140, "La Liga" },
			{ 78,  "Bundesliga" },
			{ 135, "Serie A" },
			{ 61,  "Ligue 1" },
			{ 88,  "Eredivisie" },
			{ 94,  "Liga Portugal" },
			{ 106, "Polish League Ekstraklasa" },
			{ 207, "Swiss League" },
			{ 172, "Parva Liga" },
			{ 491, "Vtora Liga" },
		};
	}
}
