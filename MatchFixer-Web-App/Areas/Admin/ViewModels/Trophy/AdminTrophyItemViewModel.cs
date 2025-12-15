using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Trophy
{
	public class AdminTrophyItemViewModel
	{
		public int TrophyId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string IconUrl { get; set; }
		public TrophyLevel Level { get; set; }
		public DateTime AwardedOn { get; set; }
	}

}
