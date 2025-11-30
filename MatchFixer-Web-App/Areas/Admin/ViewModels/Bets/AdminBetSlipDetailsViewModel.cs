using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Bets
{
	public class AdminBetSlipDetailsViewModel
	{
		public Guid Id { get; set; }
		public BetStatus SlipStatus { get; set; }

		public decimal Stake { get; set; }
		public decimal? WinAmount { get; set; }
		public decimal? PotentialReturn { get; set; }

		public string StatusBadge =>
			SlipStatus switch
			{
				BetStatus.Won => "bg-success",
				BetStatus.Lost => "bg-danger",
				BetStatus.Voided => "bg-secondary",
				BetStatus.Pending => "bg-warning text-dark",
				_ => "bg-light text-dark"
			};

		public List<AdminBetSlipSelectionDto> Selections { get; set; } = new();
	}
}
