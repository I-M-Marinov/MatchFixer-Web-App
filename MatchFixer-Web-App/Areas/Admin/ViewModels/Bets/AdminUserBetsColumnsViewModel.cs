using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Bets
{
	public class AdminUserBetsColumnsViewModel
	{
		// Header
		public Guid UserId { get; set; }
		public string? UserName { get; set; }
		public string? Email { get; set; }

		// Columns
		public List<AdminBetSlipRowDto> Pending { get; set; } = new();
		public List<AdminBetSlipRowDto> Won { get; set; } = new();
		public List<AdminBetSlipRowDto> LostOrVoided { get; set; } = new();

		// Totals (optional, handy for badges)
		public int CountPending => Pending.Count;
		public int CountWon => Won.Count;
		public int CountLostOrVoided => LostOrVoided.Count;

		public decimal TotalStake => Pending.Sum(x => x.Stake) + Won.Sum(x => x.Stake) + LostOrVoided.Sum(x => x.Stake);
		public decimal TotalReturns => (Won.Sum(x => x.WinAmount ?? 0m)); // only actual won money
		public decimal TotalLost => LostOrVoided
			.Where(x => x.SlipStatus == BetStatus.Lost)
			.Sum(x => x.Stake);
		public decimal TotalSettledStake =>
			Won.Sum(x => x.Stake) +
			LostOrVoided.Where(x => x.SlipStatus == BetStatus.Lost).Sum(x => x.Stake);
		// Net profit over settled slips
		public decimal NetProfit => TotalReturns - TotalSettledStake;
	}
}
