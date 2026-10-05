using System;
using System.Collections.Generic;

namespace MatchFixer.Common.ServiceConstants
{
	/// <summary>
	/// Groups the API-Football fixture status short codes by meaning for auto-scoring.
	/// </summary>
	public static class FootballFixtureStatuses
	{
		// Match is over with a definitive result.
		public static readonly HashSet<string> Finished =
			new(StringComparer.OrdinalIgnoreCase) { "FT", "AET", "PEN" };

		// Match is currently being played.
		public static readonly HashSet<string> InProgress =
			new(StringComparer.OrdinalIgnoreCase) { "1H", "HT", "2H", "ET", "BT", "P", "LIVE", "INT" };

		// Match has not kicked off yet.
		public static readonly HashSet<string> NotStarted =
			new(StringComparer.OrdinalIgnoreCase) { "NS", "TBD" };

		// Will not produce a normal result -> void / postpone instead of scoring.
		public static readonly HashSet<string> Abandoned =
			new(StringComparer.OrdinalIgnoreCase) { "PST", "CANC", "ABD", "SUSP", "AWD", "WO" };

		public const string Penalties = "PEN";
	}
}
