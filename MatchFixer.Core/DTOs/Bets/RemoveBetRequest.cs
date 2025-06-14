namespace MatchFixer.Core.DTOs.Bets
{
	public class RemoveBetRequest
	{
		public Guid MatchId { get; set; }
		public string SelectedOption { get; set; }
	}

}
