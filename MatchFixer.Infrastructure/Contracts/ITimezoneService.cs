
namespace MatchFixer.Infrastructure.Contracts
{
	public interface ITimezoneService
	{
		IEnumerable<string> GetTimezonesForCountry(string countryCode);
		Task<bool> IsValidTimezoneAsync(string countryCode, string timezone);
	}
}


