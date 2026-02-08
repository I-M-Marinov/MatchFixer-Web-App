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

			var alias = await _db.TeamAliases
				.Include(a => a.Team)
				.ThenInclude(t => t.Aliases)
				.FirstOrDefaultAsync(a =>
					a.Alias.ToLower() == inputName.ToLower());

			return alias?.Team;
		}

	}

}