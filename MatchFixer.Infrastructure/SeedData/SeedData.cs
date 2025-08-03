using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using MatchFixer.Common.GeneralConstants;
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

		public static async Task SeedInitialTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var specialEventTrophies = new List<Trophy>
			{
				new Trophy
				{
					Name = TrophyNames.ShadowSyndicateRecruit,
					Description = "Welcome to the underworld — you've been initiated just by showing up.",
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Bronze,
					IconUrl = "/images/trophies/shadow-syndicate-recruit.png",
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

		public static async Task SeedOutcomeTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var outcomeTrophies = new List<Trophy>
			{
				new Trophy
				{
					Name = "Fixer’s Hot Streak",
					Description = "Won 3 bets in a row — the syndicate’s golden goose is on fire.",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = "/images/trophies/fixers-hot-streak.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Syndicate Sharpshooter",
					Description = "5 winning bets in a single day — you’re making bookies sweat.",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = "/images/trophies/syndicate-sharpshooter.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Rigged to Win",
					Description = "20 total winning bets — statistically suspicious, impressively efficient.",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Gold,
					IconUrl = "/images/trophies/rigged-to-win.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Fix Gone Wrong",
					Description = "Lost 3 bets in a row — even the best plans fall apart sometimes.",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = "/images/trophies/fix-gone-wrong.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Unlucky Syndicate Soldier",
					Description = "Lost 10 bets total — it’s not fixing, it’s just bad luck… right?",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = "/images/trophies/unlucky-syndicate-soldier.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Bankroll Obliterator",
					Description = "Lost 5 bets in a single day — did you even try?",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Gold,
					IconUrl = "/images/trophies/bankroll-obliterator.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "Comeback Kingpin",
					Description = "Won a bet after 5 straight losses — resilience or divine intervention?",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Platinum,
					IconUrl = "/images/trophies/comeback-kingpin.png",
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = "The Rollercoaster Rigger",
					Description = "Alternated between win and loss 4 times in a row — your luck swings like a pendulum.",
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = "/images/trophies/rollercoaster-rigger.png",
					IsHiddenUntilEarned = false
				}
			};

			foreach (var trophy in outcomeTrophies)
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
