using MatchFixer.Infrastructure.Contracts;
using NodaTime.TimeZones;
using System.Globalization;
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

		public Task<bool> IsValidTimezoneAsync(string countryCode, string timezone)
		{
			// Validate in-process against the same timezone database the API uses.
			// (Previously this made an HTTP call to a hardcoded https://localhost:7190/
			//  which does not exist in the deployed container, so every timezone change
			//  was rejected with "Time Zone is missing or incorrect".)
			if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(timezone))
				return Task.FromResult(false);

			var isValid = GetTimezonesForCountry(countryCode).Contains(timezone);
			return Task.FromResult(isValid);
		}

		public DateTime? ConvertToUserTime(DateTime? utcTime, string timeZoneId)
		{
			if (!utcTime.HasValue)
				return null;

			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
				return TimeZoneInfo.ConvertTimeFromUtc(
					DateTime.SpecifyKind(utcTime.Value, DateTimeKind.Utc),
					tz
				);
			}
			catch (TimeZoneNotFoundException)
			{
				return utcTime;
			}
		}

		public string FormatForUser(DateTime? utcTime, string timeZoneId, string culture = "en-US")
		{
			var localTime = ConvertToUserTime(utcTime, timeZoneId);

			if (!localTime.HasValue)
				return "TBA";

			return localTime.Value.ToString(
				"g",
				CultureInfo.GetCultureInfo(culture)
			);
		}

		public string FormatForUser(DateTime? utcTime, string timeZoneId)
		{
			return FormatForUser(utcTime, timeZoneId, "en-US");
		}


		public string FormatForUserBets(DateTime? utcTime, string timeZoneId, string culture = "en-US")
		{
			var localTime = ConvertToUserTime(utcTime, timeZoneId);

			if (!localTime.HasValue)
				return "TBA";

			return localTime.Value.ToString(
				"ddd, MMM d, hh:mm tt",
				CultureInfo.GetCultureInfo(culture)
			);
		}

		// overload for the Admin Area
		public string FormatForUserExact(DateTime? utcTime, string timeZoneId, string format, IFormatProvider? provider = null)
		{
			if (!utcTime.HasValue)
				return "TBA";

			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

				var local = TimeZoneInfo.ConvertTimeFromUtc(
					DateTime.SpecifyKind(utcTime.Value, DateTimeKind.Utc),
					tz
				);

				return local.ToString(format, provider ?? CultureInfo.InvariantCulture);
			}
			catch (TimeZoneNotFoundException)
			{
				return utcTime.Value.ToString(format, provider ?? CultureInfo.InvariantCulture);
			}
		}

		public DateTime ConvertFromUserTimeToUtc(DateTime localTime, string timeZoneId)
		{
			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

				return TimeZoneInfo.ConvertTimeToUtc(
					DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified),
					tz
				);
			}
			catch (TimeZoneNotFoundException)
			{
				// fallback: assume already UTC (safe fallback)
				return DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
			}
		}


	}

}
