using MatchFixer.Infrastructure.Contracts;
using NodaTime.TimeZones;
using System.Text.Json;

namespace MatchFixer.Infrastructure.Services
{
	public class TimezoneService : ITimezoneService
	{
		private readonly HttpClient _httpClient;

		public TimezoneService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public IEnumerable<string> GetTimezonesForCountry(string countryCode)
		{
			var zones = TzdbDateTimeZoneSource.Default.ZoneLocations
				.Where(z => z.CountryCode == countryCode)
				.Select(z => z.ZoneId)
				.Distinct()
				.OrderBy(z => z);

			return zones;
		}

		public async Task<bool> IsValidTimezoneAsync(string countryCode, string timezone)
		{
			try
			{
				_httpClient.BaseAddress = new Uri("https://localhost:7190/");

				// Call the API to get valid timezones for the country
				var response = await _httpClient.GetAsync($"api/timezones/{countryCode}");

				if (response.IsSuccessStatusCode)
				{
					// Deserialize the JSON response into an array of strings (timezones)
					var jsonResponse = await response.Content.ReadAsStringAsync();
					var validTimezones = JsonSerializer.Deserialize<string[]>(jsonResponse);

					// Check if the selected timezone is in the list of valid timezones
					return validTimezones != null && validTimezones.Contains(timezone);
				}

				// Log or throw an exception if the response is not successful
				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

	}

}
