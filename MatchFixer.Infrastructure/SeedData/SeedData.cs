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
					Description = "Every legend starts with one rigged match.",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Bronze,
					MilestoneTarget = 1,
					IconUrl = "/images/trophies/rookier-rigger.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Fixer's Dozen",
					Description = "You’ve hit triple digits. Fixing matches like a true pro.",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Silver,
					MilestoneTarget = 100,
					IconUrl = "/images/trophies/fixers-dozen.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Bet Syndicate Boss",
					Description = "Cash flows, bets grow, you run this show.",
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Gold,
					MilestoneTarget = 2000, 
	                IconUrl = "/images/trophies/bet-syndicate-boss.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Ultimate Grinder",
					Description = "If placing bets were a job, you'd already be CEO of MatchFix Inc.",
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

		public static async Task SeedTimeBasedTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var timeBasedTrophies = new List<Trophy>
			{
				new Trophy
				{
					Name = "Fixer at Dawn",
					Description = "Up before the bookies — rigging the odds with your morning brew.",
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = "/images/trophies/fixer-at-dawn.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Midnight Fixer",
					Description = "While others sleep, you’re cooking bets in the moonlight.",
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Silver,
					IconUrl = "/images/trophies/midnight-fixer.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Weekend Wager Warlord",
					Description = "Weekends are for shady wagers and syndicate profits.",
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Gold,
					IconUrl = "/images/trophies/weekend-wager-warlord.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Last-Minute Leak",
					Description = "Betting at the buzzer with 'insider' info in hand.",
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Platinum,
					IconUrl = "/images/trophies/last-minute-leak.png",
					IsHiddenUntilEarned = false
				}
			};

			foreach (var trophy in timeBasedTrophies)
			{
				bool exists = await dbContext.Trophies.AnyAsync(t => t.Name == trophy.Name);
				if (!exists)
				{
					dbContext.Trophies.Add(trophy);
				}
			}

			await dbContext.SaveChangesAsync();
		}

		public static async Task SeedSpecialEventTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var specialEventTrophies = new List<Trophy>
	{
		new Trophy
		{
			Name = "Silent Bet, Shady Night",
			Description = "Placed a bet on Christmas Day – even Santa can’t fix this outcome.",
			Type = TrophyType.SpecialEvent,
			Level = TrophyLevel.Bronze,
			ExpirationDate = new DateTime(2025, 12, 26), // expires after Dec 25, 2025
			IconUrl = "/images/trophies/silent-bet-shady-night.png",
			IsHiddenUntilEarned = false
		},
		new Trophy
		{
			Name = "Fixmas Miracle",
			Description = "Placed 3 bets on Christmas Day – triple the cheer, triple the rigging.",
			Type = TrophyType.SpecialEvent,
			Level = TrophyLevel.Silver,
			ExpirationDate = new DateTime(2025, 12, 26), // expires after Dec 25, 2025
			IconUrl = "/images/trophies/fixmas-miracle.png",
			IsHiddenUntilEarned = false
		},
		new Trophy
		{
			Name = "New Year, New Fix",
			Description = "Started the year with a wager – resolutions are temporary, betting is forever.",
			Type = TrophyType.SpecialEvent,
			Level = TrophyLevel.Bronze,
			IconUrl = "/images/trophies/new-year-new-fix.png",
			IsHiddenUntilEarned = false
		},
		new Trophy
		{
			Name = "One Year, Many Fixes",
			Description = "Your betting career turns 1 – we see you, Fixer-in-Chief.",
			Type = TrophyType.SpecialEvent,
			Level = TrophyLevel.Gold,
			IconUrl = "/images/trophies/one-year-many-fixes.png",
			IsHiddenUntilEarned = false
		},
		new Trophy
		{
			Name = "Summer Syndicate Slam",
			Description = "10 shady bets in July – when the sun’s out, the fixes come out to play.",
			Type = TrophyType.SpecialEvent,
			Level = TrophyLevel.Silver,
			IconUrl = "/images/trophies/summer-syndicate-slam.png",
			IsHiddenUntilEarned = false
		}
	};

			foreach (var trophy in specialEventTrophies)
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
