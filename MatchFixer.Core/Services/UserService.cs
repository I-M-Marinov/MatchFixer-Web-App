using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Core.Contracts;

namespace MatchFixer.Core.Services
{
	public class UserService : IUserService
	{

		private readonly MatchFixerDbContext _context;

		public UserService(MatchFixerDbContext context)
		{
			_context = context;
		}


		public async Task<Guid> GetOrCreateDefaultImageAsync()
		{
			var defaultImage = await _context.ProfilePictures
				.FirstOrDefaultAsync(img => img.Id == DefaultImageId);

			if (defaultImage == null)
			{
				defaultImage = new ProfilePicture
				{
					Id = DefaultImageId,
					ImageUrl = DefaultImagePath,
					PublicId = DefaultPublicId
				};

				_context.ProfilePictures.Add(defaultImage);
				await _context.SaveChangesAsync();
			}

			return defaultImage.Id;
		}
	}
}
