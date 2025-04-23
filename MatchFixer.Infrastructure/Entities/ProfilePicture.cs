using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using static MatchFixer.Common.ValidationConstants.UserValidations;

namespace MatchFixer.Infrastructure.Entities
{
	public class ProfilePicture
	{
		[Key]
		[Comment("Unique identifier for the User Image")]
		public Guid Id { get; set; }

		[StringLength(UserImageMaxLength)]
		[Comment("Url address pointing to the User Image")]
		public string? ImageUrl { get; set; }

		[StringLength(UserImagePublicIdMaxLength)]
		[Required]
		[Comment("Key used in Cloudinary to determine validity of the User Image")]
		public string PublicId { get; set; } = null!;
		public virtual ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();

	}

}
