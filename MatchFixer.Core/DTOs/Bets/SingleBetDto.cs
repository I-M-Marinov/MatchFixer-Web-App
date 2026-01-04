namespace MatchFixer.Core.DTOs.Bets
{
	public class SingleBetDto
	{
		public Guid MatchId { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public string SelectedOption { get; set; } // "Home", "Draw", "Away"
		public decimal Odds { get; set; }
		public string Outcome { get; set; } = string.Empty;
		public string? Status { get; set; }
	}
}
