namespace MatchFixer.Core.DTOs.Bets
{
	public class PagedBetSlipsDTO
	{
		public string Status { get; set; } = "Pending";

		public List<UserBetSlipDTO> Slips { get; set; } = new();

		public int Page { get; set; } = 1; // defaulting to the first page 
		public int PageSize { get; set; } = 15; // defaulting to the 15 betslips per page option 
		public int TotalCount { get; set; }

		public int TotalPages =>
			PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

		// Full-status aggregate (not just the current page) used for the summary card.
		public decimal SummaryAmount { get; set; }
	}
}
