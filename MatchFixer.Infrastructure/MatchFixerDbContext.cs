using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;


namespace MatchFixer.Infrastructure
{
	public class MatchFixerDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
	{
		public MatchFixerDbContext(DbContextOptions<MatchFixerDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<MatchEvent> MatchEvents { get; set; }
		public virtual DbSet<Bet> Bets { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Bet>()
				.HasOne(b => b.User)
				.WithMany(u => u.Bets) 
				.HasForeignKey(b => b.UserId);

			builder.Entity<MatchEvent>(entity =>
			 {
			 	entity.Property(e => e.HomeOdds).HasPrecision(7, 2);
			 	entity.Property(e => e.DrawOdds).HasPrecision(7, 2);
			 	entity.Property(e => e.AwayOdds).HasPrecision(7, 2);
			 });

			builder.Entity<Bet>(entity =>
			{
				entity.Property(e => e.Amount).HasPrecision(10, 2); 
			});
		}
	}
}