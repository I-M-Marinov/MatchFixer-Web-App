using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.DTOs.UserTrophyContext
{
	public class UserTrophyContext
	{
		public List<Bet> UserBets { get; set; }
		public Guid UserId { get; set; }
		public int TotalBets { get; set; }
		public decimal TotalWagered { get; set; }
		public ApplicationUser User { get; set; }
		public DateTime Now { get; set; }
		public DbContext DbContext { get; set; }
		
	}
}
