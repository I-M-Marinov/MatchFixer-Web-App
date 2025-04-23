namespace MatchFixer.Common.ValidationConstants
{
	public static class UserValidations
	{

		// First Name of the user

		public const byte FirstNameMinLength = 2;
		public const byte FirstNameMaxLength = 25;

		// Last Name of the user

		public const byte LastNameMinLength = 3;
		public const byte LastNameMaxLength = 25;

		// Country of the user 

		public const byte CountryNameMinLength = 3;
		public const byte CountryNameMaxLength = 25;

		// Profile Picture Validations

		public const int UserImageMaxLength = 1000;
		public const int UserImagePublicIdMaxLength = 1000;

	}
}
