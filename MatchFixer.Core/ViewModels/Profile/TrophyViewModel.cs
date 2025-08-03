
using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.Profile
{
	public class TrophyViewModel
	{
		public int TrophyId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string IconUrl { get; set; } = "";  // default fallback

		public string Description { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty; // Example: "Milestone", "Time-Based", etc.

		public TrophyLevel Level { get; set; } = TrophyLevel.Bronze;

		public bool IsEarned { get; set; } = false; // Indicates if the current user has earned it

		public DateTime? AwardedOn { get; set; }  // null if not earned yet

		public bool IsHiddenUntilEarned { get; set; } = false;

		public DateTime? ExpirationDate { get; set; }

		public string? Notes { get; set; }
		public bool IsNew { get; set; }
	}
}
