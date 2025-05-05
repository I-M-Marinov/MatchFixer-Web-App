using Microsoft.EntityFrameworkCore;

using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Contracts;

namespace MatchFixer.Infrastructure.Services
{
	public class MatchGuessGameService  : IMatchGuessGameService
	{
		private readonly MatchFixerDbContext _context;
		private readonly Random _random = new();

		public MatchGuessGameService(MatchFixerDbContext context)
		{
			_context = context;
		}

		public async Task<MatchResult?> GetRandomMatchAsync()
		{
			var totalMatches = await _context.MatchResults.CountAsync();
			if (totalMatches == 0) return null;

			int skip = _random.Next(0, totalMatches);
			return await _context.MatchResults.Skip(skip).Take(1).FirstOrDefaultAsync();
		}

		public bool CheckAnswer(MatchResult match, int userHomeScore, int userAwayScore)
		{
			return match.HomeScore == userHomeScore && match.AwayScore == userAwayScore;
		}
		public async Task<MatchResult?> GetMatchByIdAsync(int id)
		{
			return await _context.MatchResults.FindAsync(id);
		}
	}

}
