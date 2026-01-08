namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class BoostViewModel
	{
		public Guid Id { get; set; }
		public decimal BoostValue { get; set; }
		public DateTime StartUtc { get; set; }
		public DateTime EndUtc { get; set; }
		public decimal? MaxStakePerBet { get; set; }
		public int? MaxUsesPerUser { get; set; }
		public string? Note { get; set; }
	}

}
