using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Common.GeneralConstants
{
	public static class ProfilePictureConstants
	{
		public const string DefaultImagePath = "/images/default-user-image.png";
		public const string DefaultPublicId = "default-public-id";
		public static readonly Guid DefaultImageId = new Guid("00000000-0000-0000-0000-000000000001");

		public const string DeletedUserImagePath = "/images/deleted-user-image.png";
		public const string DeletedUserImagePublicId = "deleted-user-public-id";
		public static readonly Guid DeletedUserImageId = new Guid("00000000-0000-0000-0000-000000000002");

		// MatchFixer Logo 

		public const string LogoUrl =
			"https://res.cloudinary.com/doorb7d6i/image/upload/v1747399160/matchFixer-logo_yfzk57.png";

		public const string NoFileUploaded = "No file was uploaded.";
		public const string ProfilePictureUploadedSuccessfully = "Profile picture uploaded successfully.";
		public const string ProfilePictureUploadFailed = "Failed to upload profile picture.";

		public const string DefaultPictureApplied = "Default profile picture already applied.";
		public const string NoProfilePictureToRemove = "No profile picture to remove.";
		public const string ProfilePictureSetToDefaultSuccessfully = "Profile picture set to default successfully.";
	}
}
