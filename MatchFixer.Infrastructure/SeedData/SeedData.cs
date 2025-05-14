using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using MatchFixer.Infrastructure.Entities;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;
using MatchFixer.Infrastructure.Services;


namespace MatchFixer.Infrastructure.SeedData
{
	public class SeedData
	{
		public static async Task SeedDefaultProfilePicture(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var defaultImage = await dbContext.ProfilePictures
				.FirstOrDefaultAsync(img => img.Id == DefaultImageId); // check if it already exists in the DB

			if (defaultImage == null) // if it does not exist create it 
			{
				defaultImage = new ProfilePicture
				{
					Id = DefaultImageId,
					ImageUrl = DefaultImagePath,
					PublicId = DefaultPublicId
				};

				dbContext.ProfilePictures.Add(defaultImage); 
				await dbContext.SaveChangesAsync();
			}

			await dbContext.SaveChangesAsync();
		}

		public static async Task SeedDeletedUsersProfilePicture(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var deletedUserImage = await dbContext.ProfilePictures
				.FirstOrDefaultAsync(img => img.Id == DeletedUserImageId); // check if it already exists in the DB

			if (deletedUserImage == null) // if it does not exist create it 
			{
				deletedUserImage = new ProfilePicture
				{
					Id = DeletedUserImageId,
					ImageUrl = DeletedUserImagePath,
					PublicId = DeletedUserImagePublicId
				};

				dbContext.ProfilePictures.Add(deletedUserImage);
				await dbContext.SaveChangesAsync();
			}

			await dbContext.SaveChangesAsync();
		}

		public static async Task SeedMatchResultsAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();
			var footballApiService = serviceProvider.GetRequiredService<FootballApiService>();

			// Safeguard: If there's already match data, skip seeding
			if (await dbContext.MatchResults.AnyAsync())
			{
				return;
			}

			await footballApiService.FetchAndSaveFixturesAsync();
		}
	}
}
