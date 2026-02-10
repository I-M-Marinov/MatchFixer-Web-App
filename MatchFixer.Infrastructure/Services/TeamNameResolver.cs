using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Infrastructure.Services
{
	public class TeamNameResolver : ITeamNameResolver
	{
		private readonly MatchFixerDbContext _db;

		public TeamNameResolver(MatchFixerDbContext db)
		{
			_db = db;
		}

		public async Task<Team?> ResolveTeamAsync(string inputName)
		{
			if (string.IsNullOrWhiteSpace(inputName))
				return null;

			inputName = inputName.Trim();

			var team = await _db.Teams
				.Include(t => t.Aliases)
				.FirstOrDefaultAsync(t => t.Name == inputName);

			return team;
		}

	}

}