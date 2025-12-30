namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class UpcomingMatchDto
	{
		public bool Selected { get; set; }

		public int ApiFixtureId { get; set; }

		public Guid? HomeTeamId { get; set; }
		public Guid? AwayTeamId { get; set; }

		public string HomeName { get; set; } = null!;
		public string AwayName { get; set; } = null!;
		public string HomeLogo { get; set; } = null!;
		public string AwayLogo { get; set; } = null!;

		public DateTimeOffset KickoffUtc { get; set; }

		public decimal HomeOdds { get; set; }
		public decimal DrawOdds { get; set; }
		public decimal AwayOdds { get; set; }
		public bool IsAlreadyImported { get; set; }
		public bool? IsManualDuplicate { get; set; }

	}
}
