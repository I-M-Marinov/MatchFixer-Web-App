namespace MatchFixer.Core.DTOs.Bets
{
	public class BetMixUpdateDto
	{
		public Guid EventId { get; set; }
		public decimal TotalStake { get; set; } 
		public int TotalBets { get; set; }
		public decimal HomePct { get; set; }
		public decimal DrawPct { get; set; }
		public decimal AwayPct { get; set; }

		public BetMixUpdateDto() { }

		public BetMixUpdateDto(Guid eventId, int totalBets, decimal totalStake, decimal homePct, decimal drawPct, decimal awayPct)
		{
			EventId = eventId;
			TotalBets = totalBets;
			TotalStake = totalStake;
			HomePct = homePct;
			DrawPct = drawPct;
			AwayPct = awayPct;
		}
	}
}
