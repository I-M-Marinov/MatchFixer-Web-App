using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MatchFixer.Infrastructure.ValidationAttributes;


namespace MatchFixer.Infrastructure.Models.Image
{

	public class ImageFileUploadModel
	{
		[Required(ErrorMessage = "Image must be selected !")]
		[AllowedExtensions(ErrorMessage = "Invalid file extension. Only JPG, JPEG and PNG files are allowed.")]
		[FileSize(MaxSizeInBytes = 10485760, ErrorMessage = "File size must not exceed 10 MegaBytes.")]
		public IFormFile FormFile { get; set; } = null!;

	}
}
