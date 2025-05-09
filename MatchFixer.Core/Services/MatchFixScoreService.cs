using MatchFixer.Core.ViewModels.MatchGuessGame;

using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class MatchFixScoreService : IMatchFixScoreService
	{
		private readonly MatchFixerDbContext _context;

		public MatchFixScoreService(MatchFixerDbContext context)
		{
			_context = context;
		}

		public async Task<List<LeaderboardEntryViewModel>> GetTopPlayersAsync(int count = 10)
		{
			return await _context.Users
				.OrderByDescending(u => u.MatchFixScore)
				.Take(count)
				.Select(u => new LeaderboardEntryViewModel
				{
					UserName = u.FullName!,
					Picture = u.ProfilePicture!.ImageUrl ?? string.Empty,
					Score = u.MatchFixScore
				})
				.ToListAsync();
		}
	}

}
