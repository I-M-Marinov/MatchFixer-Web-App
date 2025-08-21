namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class CreateOddsBoostViewModel
	{
		public Guid MatchEventId { get; set; }
		public decimal BoostValue { get; set; }
		public DateTime? StartUtc { get; set; } // optional: if null, starts immediately

		public int DurationMinutes { get; set; } = 30; // default 30 min rush
		public decimal? MaxStakePerBet { get; set; }
		public int? MaxUsesPerUser { get; set; }
		public string? Note { get; set; }
	}
}
