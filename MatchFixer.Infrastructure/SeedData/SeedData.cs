using MatchFixer.Common.Enums;
using MatchFixer.Common.FootballClubNames;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Common.Identity;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;
using static MatchFixer.Common.Identity.Permissions;


namespace MatchFixer.Infrastructure.SeedData
{
	public class SeedData
	{
		public static async Task SeedAllTrophiesAsync(IServiceProvider serviceProvider)
		{
			await SeedInitialTrophiesAsync(serviceProvider);
			await SeedMilestoneTrophiesAsync(serviceProvider);
			await SeedTimeBasedTrophiesAsync(serviceProvider);
			await SeedSpecialEventTrophiesAsync(serviceProvider);
			await SeedOutcomeTrophiesAsync(serviceProvider);
			await SeedWalletAndComboTrophiesAsync(serviceProvider); // new trophies 2026
		}
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

		public static async Task SeedWalletAndComboTrophiesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			var trophies = new List<Trophy>
			{
				// WALLET BASED 
				new Trophy
				{
					Name = TrophyNames.HighRoller,
					Description = TrophyNames.HighRollerDescription,
					Type = TrophyType.WalletBased,
					Level = TrophyLevel.Silver,
					IconUrl = TrophyNames.HighRollerImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.SyndicateBanker,
					Description = TrophyNames.SyndicateBankerDescription,
					Type = TrophyType.WalletBased,
					Level = TrophyLevel.Gold,
					IconUrl = TrophyNames.SyndicateBankerImagePath,
					IsHiddenUntilEarned = false
				},
				new Trophy
				{
					Name = TrophyNames.AllInManiac,
					Description = TrophyNames.AllInManiacDescription,
					Type = TrophyType.WalletBased,
					Level = TrophyLevel.Platinum,
					IconUrl = TrophyNames.AllInManiacImagePath,
					IsHiddenUntilEarned = true
				},

				// MORE MILESTONES

				//new Trophy
				//{
				//	Name = TrophyNames.TripleThreat,
				//	Description = TrophyNames.TripleThreatDescription,
				//	Type = TrophyType.Milestone,
				//	Level = TrophyLevel.Bronze,
				//	IconUrl = TrophyNames.TripleThreatImagePath,
				//	IsHiddenUntilEarned = false
				//},
				//new Trophy
				//{
				//	Name = TrophyNames.FiveFoldFix,
				//	Description = TrophyNames.FiveFoldFixDescription,
				//	Type = TrophyType.Milestone,
				//	Level = TrophyLevel.Silver,
				//	IconUrl = TrophyNames.FiveFoldFixImagePath,
				//	IsHiddenUntilEarned = false
				//},
				//new Trophy
				//{
				//	Name = TrophyNames.SyndicateArchitect,
				//	Description = TrophyNames.SyndicateArchitectDescription,
				//	Type = TrophyType.Milestone,
				//	Level = TrophyLevel.Gold,
				//	IconUrl = TrophyNames.SyndicateArchitectImagePath,
				//	IsHiddenUntilEarned = true
				//}
			};

			foreach (var trophy in trophies)
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
			var footballApiService = serviceProvider.GetRequiredService<IFootballApiService>();

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
			var footballApiService = serviceProvider.GetRequiredService<IFootballApiService>();

			// Safeguard: If there's already teams data, skip seeding
			if (await dbContext.Teams.AnyAsync())
			{
				return;
			}

			await footballApiService.FetchAndSaveTeamsAsync();
		}

		public static async Task SeedRolesAndAdminAsync(IServiceProvider sp)
		{
			var cfg = sp.GetRequiredService<IConfiguration>();

			var email = cfg["Admin:Email"];
			var password = cfg["Admin:Password"];
			var firstName = cfg["Admin:FirstName"] ?? "Site";
			var lastName = cfg["Admin:LastName"] ?? "Admin";
			var country = cfg["Admin:Country"] ?? "BG";
			var tz = cfg["Admin:TimeZone"] ?? "Europe/Sofia";
			var dob = DateTime.TryParse(cfg["Admin:DateOfBirth"], out var dt)
				? dt
				: new DateTime(1990, 1, 1);

			var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
			var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

			// Ensure roles exist
			foreach (var role in new[] { Roles.Admin, Roles.Moderator })
			{
				if (!await roleMgr.RoleExistsAsync(role))
				{
					await roleMgr.CreateAsync(new IdentityRole<Guid>(role));
				}
			}
			
			// Ensure admin user exists
			var admin = await userMgr.FindByEmailAsync(email);

			if (admin == null)
			{	// if it does not ---> add it 
				admin = new ApplicationUser
				{
					Id = Guid.NewGuid(),
					UserName = email,
					Email = email,
					FirstName = firstName,
					LastName = lastName,
					EmailConfirmed = true,
					Country = country,
					TimeZone = tz,
					DateOfBirth = dob,
					ProfilePictureId = DefaultImageId, // add the default image to the admin user
				};

				var createResult = await userMgr.CreateAsync(admin, password);

				if (!createResult.Succeeded)
				{
					throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));
				}
			}

			// Ensure admin has the Admin role
			if (!await userMgr.IsInRoleAsync(admin, Roles.Admin))
			{
				await userMgr.AddToRoleAsync(admin, Roles.Admin);
			}

			// Ensure claims
			var existingClaims = await userMgr.GetClaimsAsync(admin);

			var neededClaims = new[]
			{
				new Claim("permission", ManageUsers),
				new Claim("permission", ManageWallets),
				new Claim("permission", ManageMatchEvents),
			};

			// Add all permissions to the Admin role
			foreach (var claim in neededClaims)
			{
				if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
				{
					await userMgr.AddClaimAsync(admin, claim);
				}
			}
		}

		public static async Task SeedUpcomingMatchEventsAsync(IServiceProvider services)
		{
			using var scope = services.CreateScope();

			var dbContext = services.GetRequiredService<MatchFixerDbContext>();
			var seeder = services.GetRequiredService<IUpcomingMatchSeederService>();

			if (await dbContext.UpcomingMatchEvents.AnyAsync())
			{
				return;
			}

			await seeder.SeedUpcomingMatchesAsync();
		}

		public static async Task SeedTeamAliasesAsync(IServiceProvider serviceProvider)
		{
			var dbContext = serviceProvider.GetRequiredService<MatchFixerDbContext>();

			if (await dbContext.TeamAliases.AnyAsync())
			{
				return;
			}

			var teams = await dbContext.Teams
				.AsNoTracking()
				.ToListAsync();

			var aliasesToInsert = new List<TeamAlias>();

			foreach (var kvp in FootballClubNames.ClubNameMap)
			{
				var officialName = kvp.Key.Trim();   
				var alias = kvp.Value.Trim();      

				var team = teams.FirstOrDefault(t =>
					string.Equals(t.Name, officialName, StringComparison.OrdinalIgnoreCase));

				if (team == null)
				{
					continue;
				}

				if (string.Equals(alias, officialName, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				aliasesToInsert.Add(new TeamAlias
				{
					Alias = alias,
					TeamId = team.Id,
					Source = "StaticDictionarySeed"
				});
			}

			if (aliasesToInsert.Count > 0)
			{
				await dbContext.TeamAliases.AddRangeAsync(aliasesToInsert);
				await dbContext.SaveChangesAsync();
			}
		}

	}
}
