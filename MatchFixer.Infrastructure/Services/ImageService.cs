using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Models.Image;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MatchFixer.Infrastructure.Services
{
	public class ImageService : IImageService
	{
		private readonly Cloudinary _cloudinary;

		public ImageService(IConfiguration configuration)
		{
			var account = new Account(
				configuration["CloudinarySettings:CloudName"],
				configuration["CloudinarySettings:ApiKey"],
				configuration["CloudinarySettings:ApiSecret"]
			);
			_cloudinary = new Cloudinary(account);
			_cloudinary.Api.Secure = true;

		}

		public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
		{
			var uploadResult = new ImageUploadResult();

			if (file.Length > 0)
			{
				using (var stream = file.OpenReadStream())
				{
					var uploadParams = new ImageUploadParams()
					{
						File = new FileDescription(file.FileName, stream),
						Transformation = new Transformation()
							.Width(500) // Final square width
							.Height(500) // Final square height
							.Crop("fill") // Crop to fill the square
							.Gravity("auto") // Center subject automatically (or use "face" for profile pictures)
					};

					uploadResult = await _cloudinary.UploadAsync(uploadParams);
				}
			}

			return uploadResult;
		}
		public async Task<ImageResult> DeleteImageAsync(string publicId)
		{
			var deletionParams = new DeletionParams(publicId);
			var result = await _cloudinary.DestroyAsync(deletionParams);


			return new ImageResult
			{
				IsSuccess = result.StatusCode == HttpStatusCode.OK,
				Message = result.StatusCode == HttpStatusCode.OK ? "Image deleted successfully." : "Image deletion failed."
			};
		}
	}
}
