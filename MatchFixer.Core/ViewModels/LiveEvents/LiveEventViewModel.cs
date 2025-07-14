namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class LiveEventViewModel
	{
		public Guid Id { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public DateTime KickoffTime { get; set; }
		public decimal HomeWinOdds { get; set; }
		public decimal DrawOdds { get; set; }
		public decimal AwayWinOdds { get; set; }
		public string? HomeTeamLogoUrl { get; set; }
		public string? AwayTeamLogoUrl { get; set; }
		public bool IsDerby { get; set; }
		public string UserTimeZone { get; set; }
		public bool IsCancelled { get; set; }
		public string MatchStatus { get; set; } = "Scheduled"; 


	}
}