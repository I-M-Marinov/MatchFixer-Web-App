using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace MatchFixer.Infrastructure.ValidationAttributes
{
	public class AllowedExtensions : ValidationAttribute
	{
		private readonly string[] _validExtensions;

		private static readonly string[] ValidImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
		private static readonly string[] AdminValidImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

		public AllowedExtensions()
		{
			_validExtensions = ValidImageExtensions;
		}
		public AllowedExtensions(bool isAdmin)
		{
			_validExtensions = isAdmin ? AdminValidImageExtensions : ValidImageExtensions;
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			var file = value as IFormFile;

			if (file != null)
			{
				var extension = Path.GetExtension(file.FileName).ToLower();
				if (!_validExtensions.Contains(extension))
				{
					return new ValidationResult(ErrorMessage ?? "Invalid file extension.");
				}
			}

			return ValidationResult.Success!;
		}
	}
}
