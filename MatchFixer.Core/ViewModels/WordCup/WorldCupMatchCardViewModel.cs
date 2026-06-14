using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.WordCup
{
	public class WorldCupMatchCardViewModel
	{
		public int MatchId { get; set; }

		public string HomeTeam { get; set; } = null!;

		public string AwayTeam { get; set; } = null!;

		public string HomeLogo { get; set; } = null!;

		public string AwayLogo { get; set; } = null!;

		public DateTime? MatchDate { get; set; }

		public int? HomeScore { get; set; }

		public int? AwayScore { get; set; }

		public bool IsFinished { get; set; }

		public bool IsLive { get; set; }

		public bool IsOngoing { get; set; }

		public string? Winner { get; set; }

		public WorldCupStage Stage { get; set; }

		public bool IsAvailableForBetting { get; set; }

		public Guid? MatchEventId { get; set; }

	}
}
