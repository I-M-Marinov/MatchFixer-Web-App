using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class UserTrophy
	{
		public int Id { get; set; }

		public Guid UserId { get; set; }
		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }  

		public int TrophyId { get; set; }
		[ForeignKey(nameof(TrophyId))]
		public Trophy Trophy { get; set; }  

		public DateTime AwardedOn { get; set; } = DateTime.UtcNow; // set the time at now UTC 

		[StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")] 
		public string? Notes { get; set; }
	}
}
