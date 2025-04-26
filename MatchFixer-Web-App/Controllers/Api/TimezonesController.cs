using MatchFixer.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers.Api
{
	[Route("api/[controller]")]
	[ApiController]
	public class TimezonesController : ControllerBase
	{
		private readonly ITimezoneService _timezoneService;

		public TimezonesController(ITimezoneService timezoneService)
		{
			_timezoneService = timezoneService;
		}

		[HttpGet("{countryCode}")]
		public IActionResult Get(string countryCode)
		{
			var zones = _timezoneService.GetTimezonesForCountry(countryCode);
			return Ok(zones);
		}
	}

}
