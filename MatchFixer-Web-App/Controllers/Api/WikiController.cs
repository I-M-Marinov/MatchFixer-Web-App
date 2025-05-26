using MatchFixer.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.FootballClubNames.FootballClubNames;

namespace MatchFixer_Web_App.Controllers.Api
{
	[Route("api/wiki")]
	[ApiController]
	public class WikiController : ControllerBase
	{
		private readonly IWikipediaService _wikipediaService;
		private readonly ILogger<WikiController> _logger;

		public WikiController(
			IWikipediaService wikipediaService,
			ILogger<WikiController> logger)
		{
			_wikipediaService = wikipediaService;
			_logger = logger;
		}

		[ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "teamName" })]
		[HttpGet("{teamName}")]
		public async Task<IActionResult> GetTeamInfo(string teamName)
		{
			try
			{

				if (string.IsNullOrWhiteSpace(teamName))
				{
					return BadRequest("Team name cannot be empty");
				}

				var officialName = GetOfficialName(teamName);
				var info = await _wikipediaService.GetTeamInfoAsync(officialName);

				if (info == null || string.IsNullOrEmpty(info.Name))
				{
					_logger.LogWarning($"Team not found: {teamName}");
					return NotFound(new { Error = "Team information not available" });
				}

				return Ok(new
				{
					name = info.Name,
					summary = info.Summary,
					imageUrl = info.ImageUrl,
					wikipediaUrl = info.WikipediaUrl
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error fetching Wikipedia info for {teamName}");
				return StatusCode(500, new
				{
					Error = "Exception occurred",
					Detail = ex.Message
				});
			}
		}
	}
}