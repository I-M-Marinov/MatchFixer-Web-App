
using System.ComponentModel.DataAnnotations;
using MatchFixer.Core.ViewModels.LiveEvents;

namespace MatchFixer.Core.ValidationAttributes
{
	public class DifferentTeamsAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			var model = (MatchEventFormModel)validationContext.ObjectInstance;

			if (model.HomeTeamId == model.AwayTeamId && model.HomeTeamId != Guid.Empty)
			{
				return new ValidationResult("The home and away team cannot be the same.");
			}

			return ValidationResult.Success!;
		}
	}
}
