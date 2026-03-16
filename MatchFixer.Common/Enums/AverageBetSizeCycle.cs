using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Common.Enums
{
	public enum AverageBetSizeCycle
	{
		[Display(Name = "Today")]
		Today = 0,
		[Display(Name = "Yesterday")]
		Yesterday = 1,
		[Display(Name = "This Week")]
		ThisWeek = 2,
	}
}
