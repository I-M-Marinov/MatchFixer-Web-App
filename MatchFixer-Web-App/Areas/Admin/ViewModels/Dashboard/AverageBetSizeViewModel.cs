using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class AverageBetSizeViewModel
	{
		public AverageBetSizeCycle Period { get; set; }
		public decimal AverageAmount { get; set; }
	}
}
