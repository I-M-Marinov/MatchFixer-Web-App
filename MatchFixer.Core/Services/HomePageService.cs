using MatchFixer.Common;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Index;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace MatchFixer.Core.Services
{
	public class HomePageService: IHomePageService
	{
		private readonly MatchFixerDbContext _dbContext;
		private static readonly Random random = new();


		public HomePageService(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<RecentBigWinViewModel>> GetRecentBigWinsAsync()
		{
			var query = _dbContext.BetSlips
				.AsNoTracking()
				.Where(bs =>
					bs.IsSettled &&
					bs.WinAmount != null &&
					bs.WinAmount > 0 &&
					bs.Bets.Count >= 3)
				.OrderByDescending(bs => bs.BetTime)
				.Take(20)
				.Select(bs => new RecentBigWinViewModel
				{
					BetSlipId = bs.Id,

					Username = "", // do not assign username or name yet 

					WinAmount = bs.WinAmount!.Value,

					StakeAmount = bs.Amount,

					LegsCount = bs.Bets.Count,

					BetTime = bs.BetTime,

					Odds = bs.Bets
						.Select(b => b.Odds)
						.ToList(),

					Bets = bs.Bets.Select(b => new RecentBigWinBetViewModel
					{
						HomeTeam = b.MatchEvent.HomeTeam.Name,
						AwayTeam = b.MatchEvent.AwayTeam.Name,
						HomeTeamImageUrl = b.MatchEvent.HomeTeam.LocalLogoUrl,
						AwayTeamImageUrl = b.MatchEvent.AwayTeam.LocalLogoUrl,
						Pick = b.Pick,
						Odds = b.Odds,
						Status = b.Status,

						HomeScore = b.MatchEvent.LiveResult != null
							? b.MatchEvent.LiveResult.HomeScore
							: null,

						AwayScore = b.MatchEvent.LiveResult != null
							? b.MatchEvent.LiveResult.AwayScore
							: null,

						HomeWonOnPenalties = b.MatchEvent.LiveResult != null
							? b.MatchEvent.LiveResult.HomeWonOnPenalties
							: null

					}).ToList()
				})
				.ToListAsync();

			var slips = await query;

			foreach (var slip in slips)
			{	
				// add the fake names randomly here 
				slip.Username =
					IndexConstants.FakeBigWinUsernames[
						Math.Abs(slip.BetSlipId.GetHashCode()) %
						IndexConstants.FakeBigWinUsernames.Count
					];

				slip.TotalOdds =
					slip.Odds.Aggregate(1m, (x, y) => x * y);
			}

			return slips;
		}
	}
}
