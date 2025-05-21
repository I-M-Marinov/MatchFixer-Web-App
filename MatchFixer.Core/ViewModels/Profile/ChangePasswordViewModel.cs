using System.ComponentModel.DataAnnotations;
using static MatchFixer.Common.ServiceConstants.PasswordRequirements;
using static MatchFixer.Common.ValidationConstants.ChangePasswordValidations;

namespace MatchFixer.Core.ViewModels.Profile
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = CurrentPasswordIsRequired)]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = null!;

		[Required(ErrorMessage = NewPasswordIsRequired)]
		[DataType(DataType.Password)]
		[RegularExpression(RegexPattern, ErrorMessage = ErrorMessage)]
		public string NewPassword { get; set; } = null!;

		[Required(ErrorMessage = ConfirmationPasswordIsRequired)]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = PasswordsDoNotMatch)]
		public string ConfirmPassword { get; set; } = null!;
	}
}
