using System.ComponentModel.DataAnnotations;
using static MatchFixer.Common.ServiceConstants.PasswordRequirements;

namespace MatchFixer.Core.ViewModels.Profile
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Current password is required.")]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = null!;

		[Required(ErrorMessage = "The new password field is required.")]
		[DataType(DataType.Password)]
		[RegularExpression(RegexPattern, ErrorMessage = ErrorMessage)]
		public string NewPassword { get; set; } = null!;

		[Required(ErrorMessage = "The confirmation password field is required.")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
