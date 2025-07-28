using System.ComponentModel.DataAnnotations;
using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Entities
{
	public class Trophy
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; } = string.Empty;

		public string IconUrl { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public TrophyType Type { get; set; }  

		public TrophyLevel Level { get; set; } = TrophyLevel.Bronze;  // fall back to Bronze to begin with

		public int? MilestoneTarget { get; set; } 

		public bool IsHiddenUntilEarned { get; set; } = false;
		public DateTime? ExpirationDate { get; set; } // specifically for time sensitive trophies or milestones ( UTC TIME ) 
		public ICollection<UserTrophy> AwardedUsers { get; set; } = new List<UserTrophy>(); // all users with that have the trophy 


	}
}
