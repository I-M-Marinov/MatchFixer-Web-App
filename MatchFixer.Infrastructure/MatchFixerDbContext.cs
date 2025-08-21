using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace MatchFixer.Infrastructure
{
	public class MatchFixerDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
	{
		public MatchFixerDbContext(DbContextOptions<MatchFixerDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
 		public virtual DbSet<ProfilePicture> ProfilePictures { get; set; }
		public virtual DbSet<MatchEvent> MatchEvents { get; set; }
		public virtual DbSet<Bet> Bets { get; set; }
		public virtual DbSet<BetSlip> BetSlips { get; set; }
		public virtual DbSet<Team> Teams { get; set; }
		public virtual DbSet<MatchResult> MatchResults { get; set; }
		public virtual DbSet<Wallet> Wallets { get; set; }
		public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }
		public virtual DbSet<LiveMatchResult> LiveMatchResults { get; set; }
		public virtual DbSet<Trophy> Trophies { get; set; }
		public virtual DbSet<UserTrophy> UserTrophies { get; set; }
		public virtual DbSet<MatchEventLog> MatchEventLogs { get; set; }
		public virtual DbSet<OddsBoost> OddsBoosts { get; set; }


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<BetSlip>()
				.HasOne(bs => bs.User)
				.WithMany(u => u.BetSlips)
				.HasForeignKey(bs => bs.UserId);

			builder.Entity<Bet>()
				.HasOne(b => b.BetSlip)
				.WithMany(bs => bs.Bets)
				.HasForeignKey(b => b.BetSlipId)
				.OnDelete(DeleteBehavior.Cascade); // when a slip is deleted, delete its bets

			builder.Entity<Bet>()
				.HasOne(b => b.MatchEvent)
				.WithMany(me => me.Bets)  
				.HasForeignKey(b => b.MatchEventId);

			builder.Entity<MatchEvent>(entity =>
			{
				entity.Property(e => e.HomeOdds).HasPrecision(7, 2);
				entity.Property(e => e.DrawOdds).HasPrecision(7, 2);
				entity.Property(e => e.AwayOdds).HasPrecision(7, 2);
			});

			builder.Entity<Bet>(entity =>
			{
				entity.Property(e => e.Odds).HasPrecision(7, 2);  // new property for odds per bet
			});

			builder.Entity<BetSlip>(entity =>
			{
				entity.Property(e => e.Amount).HasPrecision(10, 2);
				entity.Property(e => e.WinAmount).HasPrecision(10, 2);
			});

			builder.Entity<Team>()
				.HasIndex(t => t.Name)
				.IsUnique();

			builder.Entity<MatchResult>()
				.HasOne(m => m.HomeTeam)
				.WithMany(t => t.HomeMatches)
				.HasForeignKey(m => m.HomeTeamId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<MatchResult>()
				.HasOne(m => m.AwayTeam)
				.WithMany(t => t.AwayMatches)
				.HasForeignKey(m => m.AwayTeamId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<UserTrophy>()
				.HasIndex(ut => new { ut.UserId, ut.TrophyId })
				.IsUnique(); // a user can get only one unique trophy 

			builder.Entity<OddsBoost>()
				.HasIndex(x => new { x.MatchEventId, x.StartUtc, x.EndUtc, x.IsActive });

			builder.Entity<OddsBoost>()
				.HasOne(x => x.MatchEvent) // One oddsboost belongs to one MatchEvent
				.WithMany(e => e.OddsBoosts) // One MatchEvent can have many OddsBoosts
				.HasForeignKey(x => x.MatchEventId);

			builder.Entity<OddsBoost>(entity =>
			{
				entity.Property(e => e.BoostValue)
					.HasPrecision(5, 2); 

				entity.Property(e => e.MaxStakePerBet)
					.HasPrecision(18, 2);
			});
		}
	}
}