namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class PlatformExposureViewModel
	{
		public decimal TotalStakesToday { get; set; }
		public decimal PotentialPayout { get; set; }
		public decimal Exposure => PotentialPayout - TotalStakesToday;
	}
}
