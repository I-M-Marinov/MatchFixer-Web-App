﻿using Microsoft.EntityFrameworkCore;

using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

using static MatchFixer.Common.DerbyLookup.DerbyLookup;

#nullable disable

namespace MatchFixer.Core.Services
{
	public class MatchEventService : IMatchEventService
	{
		private readonly MatchFixerDbContext _dbContext;

		public MatchEventService(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<LiveEventViewModel>> GetLiveEventsAsync()
		{
			var events = await _dbContext.MatchEvents
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.OrderBy(e => e.MatchDate)
				.Select(e => new LiveEventViewModel
				{
					Id = Guid.NewGuid(),
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					KickoffTime = e.MatchDate,
					HomeWinOdds = e.HomeOdds ?? 0,
					DrawOdds = e.DrawOdds ?? 0,
					AwayWinOdds = e.AwayOdds ?? 0,
					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
					IsDerby = e.IsDerby
				})
				.AsNoTracking()
				.ToListAsync();

			return events;
		}

		public async Task AddEventAsync(MatchEventFormModel model)
		{
			var homeTeam = await _dbContext.Teams.FindAsync(model.HomeTeamId);
			var awayTeam = await _dbContext.Teams.FindAsync(model.AwayTeamId);

			if (homeTeam == null || awayTeam == null)
			{
				throw new Exception("Home or Away team does not exist!");
			}

			var formattedDateAndTime = model.MatchDate.ToString("f", CultureInfo.InvariantCulture);

			var isDerby = IsDerby((int)homeTeam.TeamId, (int)awayTeam.TeamId);


			var matchEvent = new MatchEvent
			{
				Id = Guid.NewGuid(),
				HomeTeamId = model.HomeTeamId,
				AwayTeamId = model.AwayTeamId,
				MatchDate = DateTime.Parse(formattedDateAndTime),
				HomeOdds = model.HomeOdds,
				DrawOdds = model.DrawOdds,
				AwayOdds = model.AwayOdds,
				IsDerby = isDerby
			};

			await _dbContext.MatchEvents.AddAsync(matchEvent);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<string> GetTeamLogo(string name)
		{
			var logo = await _dbContext.Teams
				.Where(t => t.Name == name)
				.Select(t => t.LogoUrl.ToString())
				.AsNoTracking()
				.FirstOrDefaultAsync();

			return logo;
		}

		public async Task<Dictionary<string, List<SelectListItem>>> GetTeamsGroupedByLeagueAsync()
		{
			var teams = await _dbContext.Teams
				.OrderBy(t => t.LeagueName)
				.ThenBy(t => t.Name)
				.AsNoTracking()
				.ToListAsync();

			var teamsByLeague = teams
				.GroupBy(t => t.LeagueName)
				.ToDictionary(
					group => group.Key,
					group => group.Select(t => new SelectListItem
					{
						Value = t.Id.ToString(),
						Text = $"{t.Name}|{t.LogoUrl}"
					}).ToList()
				);

			return teamsByLeague;
		}


	}
}
