﻿using System.Net;
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
							.Quality(100)
							.Width(2000) // Set maximum width for the main image
							.Height(2000) // Set maximum height for the main image
							.Crop("limit") // Ensure the main image does not exceed these dimensions
					};

					uploadResult = await _cloudinary.UploadAsync(uploadParams);
				}
			}

			return uploadResult;
		}
		public async Task<DeleteImageResult> DeleteImageAsync(string publicId)
		{
			var deletionParams = new DeletionParams(publicId);
			var result = await _cloudinary.DestroyAsync(deletionParams);


			return new DeleteImageResult
			{
				IsSuccess = result.StatusCode == HttpStatusCode.OK,
				Message = result.StatusCode == HttpStatusCode.OK ? "Image deleted successfully." : "Image deletion failed."
			};
		}
	}
}
