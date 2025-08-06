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
					Name = TrophyNames.RookieRigger,
					Description = TrophyNames.RookieRiggerDescription,
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Bronze,
					MilestoneTarget = 1,
					IconUrl = TrophyNames.RookieRiggerImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.FixersDozen,
					Description = TrophyNames.FixersDozenDescription,
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Silver,
					MilestoneTarget = 100,
					IconUrl = TrophyNames.FixersDozenImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.BetSyndicateBoss,
					Description = TrophyNames.BetSyndicateBossDescription,
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Gold,
					MilestoneTarget = 2000,
					IconUrl = TrophyNames.BetSyndicateBossImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.UltimateGrinder,
					Description = TrophyNames.UltimateGrinderDescription,
					Type = TrophyType.Milestone,
					Level = TrophyLevel.Platinum,
					MilestoneTarget = 1000,
					IconUrl = TrophyNames.UltimateGrinderImagePath,
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
					Name = TrophyNames.FixerAtDawn,
					Description = TrophyNames.FixerAtDawnDescription,
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = TrophyNames.FixerAtDawnImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.MidnightFixer,
					Description = TrophyNames.MidnightFixerDescription,
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.MidnightFixerImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.WeekendWagerWarlord,
					Description = TrophyNames.WeekendWagerWarlordDescription,
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Gold,
					IconUrl = TrophyNames.WeekendWagerWarlordImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.LastMinuteLeak,
					Description = TrophyNames.LastMinuteLeakDescription,
					Type = TrophyType.TimeBased,
					Level = TrophyLevel.Platinum,
					IconUrl = TrophyNames.LastMinuteLeakImagePath,
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
					Name = TrophyNames.SilentBetShadyNight,
					Description = TrophyNames.SilentBetShadyNightDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Bronze,
					ExpirationDate = new DateTime(2025, 12, 26),
					IconUrl = TrophyNames.SilentBetShadyNightImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.FixmasMiracle,
					Description = TrophyNames.FixmasMiracleDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Silver,
					ExpirationDate = new DateTime(2025, 12, 26),
					IconUrl = TrophyNames.FixmasMiracleImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.NewYearNewFix,
					Description = TrophyNames.NewYearNewFixDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Bronze,
					IconUrl = TrophyNames.NewYearNewFixImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.OneYearManyFixes,
					Description = TrophyNames.OneYearManyFixesDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Gold,
					IconUrl = TrophyNames.OneYearManyFixesImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.SummerSyndicateSlam,
					Description = TrophyNames.SummerSyndicateSlamDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.SummerSyndicateSlamImagePath,
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
					Description = TrophyNames.ShadowSyndicateRecruitDescription,
					Type = TrophyType.SpecialEvent,
					Level = TrophyLevel.Bronze,
					IconUrl = TrophyNames.ShadowSyndicateRecruitImagePath,
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
					Name = TrophyNames.FixersHotStreak,
					Description = TrophyNames.FixersHotStreakDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = TrophyNames.FixersHotStreakImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.SyndicateSharpshooter,
					Description = TrophyNames.SyndicateSharpshooterDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.SyndicateSharpshooterImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.RiggedToWin,
					Description = TrophyNames.RiggedToWinDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Gold,
					IconUrl = TrophyNames.RiggedToWinImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.FixGoneWrong,
					Description = TrophyNames.FixGoneWrongDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Bronze,
					IconUrl = TrophyNames.FixGoneWrongImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.UnluckySyndicateSoldier,
					Description = TrophyNames.UnluckySyndicateSoldierDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.UnluckySyndicateSoldierImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.BankrollObliterator,
					Description = TrophyNames.BankrollObliteratorDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Gold,
					IconUrl = TrophyNames.BankrollObliteratorImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.ComebackKingpin,
					Description = TrophyNames.ComebackKingpinDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Platinum,
					IconUrl = TrophyNames.ComebackKingpinImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.RollercoasterRigger,
					Description = TrophyNames.RollercoasterRiggerDescription,
					Type = TrophyType.OutcomeBased,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.RollercoasterRiggerImagePath,
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
