using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;


namespace MatchFixer.Infrastructure.SeedData
{
	public class SeedData
	{
		public static async Task SeedMilestoneTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var milestoneTrophies = new List<Trophy>
			{
				new Trophy
				{
					Name = "Rookie Rigger",
					Description = "Place your first bet",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Bronze,
					MilestoneTarget = 1,
					IconUrl = "/images/trophies/rookier-rigger.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Fixer's Dozen",
					Description = "Place 100 total bets",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Silver,
					MilestoneTarget = 100,
					IconUrl = "/images/trophies/fixers-dozen.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Bet Syndicate Boss",
					Description = "Wager a total of 2000 euros",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Gold,
					MilestoneTarget = 2000, 
	                IconUrl = "/images/trophies/bet-syndicate-boss.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Ultimate Grinder",
					Description = "Place 1000 total bets",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Platinum,
					MilestoneTarget = 1000,
					IconUrl = "/images/trophies/ultimate-grinder.png",
					IsHiddenUntilEarned = false
				}
			};

			foreach (var trophy in milestoneTrophies)
			{
				bool exists = await dbContext.Trophies.AnyAsync(t => t.Name == trophy.Name);
				if (!exists)
				{
					dbContext.Trophies.Add(trophy);
				}
			}

			await dbContext.SaveChangesAsync();
		}
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

		public static async Task SeedTeams(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();
			var footballApiService = serviceProvider.GetRequiredService<FootballApiService>();

			// Safeguard: If there's already teams data, skip seeding
			if (await dbContext.Teams.AnyAsync())
			{
				return;
			}

			await footballApiService.FetchAndSaveTeamsAsync();
		}
	}
}
