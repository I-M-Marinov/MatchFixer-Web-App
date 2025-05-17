
namespace MatchFixer.Infrastructure.Contracts
{
	public interface ITimezoneService
	{
		IEnumerable<string> GetTimezonesForCountry(string countryCode);
		Task<bool> IsValidTimezoneAsync(string countryCode, string timezone);
		DateTime ConvertToUserTime(DateTime utcTime, string timeZoneId);
		string FormatForUser(DateTime utcTime, string timeZoneId, string culture);
	}
}


