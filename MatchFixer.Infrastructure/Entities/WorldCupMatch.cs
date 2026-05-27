using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Entities
{
	public class WorldCupMatch
	{
		public int Id { get; set; }

		public string HomeTeam { get; set; } = null!;

		public string AwayTeam { get; set; } = null!;

		public string HomeLogo { get; set; } = null!;

		public string AwayLogo { get; set; } = null!;

		public DateTime MatchDate { get; set; }

		public int? HomeScore { get; set; }

		public int? AwayScore { get; set; }

		public WorldCupStage Stage { get; set; }

		public string? GroupName { get; set; }

		public bool IsFinished { get; set; }

		public bool IsLive { get; set; }

		public bool IsKnockout { get; set; }

		public int RoundPosition { get; set; }

		public int? ApiEventId { get; set; }
	}
}
