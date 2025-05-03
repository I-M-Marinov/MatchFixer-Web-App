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
							.Width(500)
							.Height(500)
							.Crop("fill")
							.Gravity("auto")
							.Chain()													// apply the image transformation before anything else 
							.Overlay(new Layer().PublicId("matchfixer_watermark"))		// watermark's public id 
							.Gravity("north_west")                                      // position the watermark  in the left top corner of the image 
							.X(25)                                                      // move the watermark 20px to the right
							.Y(10)                                                      // move the watermark 10px down 
							.Width(150)													// resize the watermark
							.Crop("scale")												// scale it 
							.Opacity(40)												// visibility of the watermark
							.Effect("brightness:30")									// slight adjustment to enhance watermark clarity 
							.Flags("layer_apply")										// ensure the watermark layer is applied after the transformations on the image are done
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
