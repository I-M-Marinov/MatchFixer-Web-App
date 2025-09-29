using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Bets
{
	public class AdminBetSlipRowDto
	{
		public Guid Id { get; set; }
		public BetStatus SlipStatus { get; set; }
		public decimal Stake { get; set; }             
		public decimal? WinAmount { get; set; }       
		public decimal? PotentialReturn { get; set; } 

		public int Selections { get; set; }

		public DateTime CreatedUtc { get; set; }
		public string CreatedDisplay { get; set; } = "";
		public DateTime? SettledUtc { get; set; }
		public string? SettledDisplay { get; set; }
	}

}
