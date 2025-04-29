using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using MatchFixer.Infrastructure.Models.Image;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface IImageService
	{
		/// <summary>
		/// Uploads an image to Cloudinary 
		/// </summary>
		Task<ImageUploadResult> UploadImageAsync(IFormFile file);
		/// <summary>
		/// Sends a publicId to Cloudinary to delete the image
		/// </summary>
		Task<ImageResult> DeleteImageAsync(string publicId);

	}
}
