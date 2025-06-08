namespace MatchFixer.Core.DTOs.Bets
{
	public class BetSlipDto
	{
		public decimal Amount { get; set; }
		public decimal TotalOdds { get; set; }
		public List<SingleBetDto> Bets { get; set; }
	}
}
