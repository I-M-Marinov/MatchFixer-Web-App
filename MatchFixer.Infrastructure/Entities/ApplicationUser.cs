using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MatchFixer.Common.ValidationConstants.UserValidations;

namespace MatchFixer.Infrastructure.Entities
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public ApplicationUser() { }

		[Required]
		[StringLength(FirstNameMaxLength)]
		[Comment("First name of the application user")]
		public string FirstName { get; set; }
		[Required]
		[StringLength(LastNameMaxLength)]
		[Comment("Last name of the application user")]
		public string LastName { get; set; }

		[NotMapped]
		public string FullName => $"{FirstName} {LastName}";

		[Required]
		[Comment("Date of birth of the application user")]
		public DateTime DateOfBirth { get; set; }
		[Required]
		[StringLength(CountryNameMaxLength)]
		[Comment("Country of origin of the application user")]
		public string Country { get; set; }
		[Comment("Time Zone of the application user")]
		public string TimeZone { get; set; }
		[Comment("Id of the profile picture of the application user")]
		public Guid? ProfilePictureId { get; set; }

		[ForeignKey(nameof(ProfilePictureId))]
		public virtual ProfilePicture? ProfilePicture { get; set; }

		[Comment("Date user was created")]
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

		[Comment("Date user was last updated")]
		public DateTime? ModifiedOn { get; set; }

		[Comment("Indicates whether the user is marked as deleted")]
		public bool IsDeleted { get; set; } = false;

		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}
}
