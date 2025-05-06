using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MatchFixer.Core.Extensions
{
	public static class SessionExtensions
	{
		public static void SetObject(this ISession session, string key, object value)
		{
			var json = JsonConvert.SerializeObject(value);
			session.SetString(key, json);
		}

		public static T GetObject<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			if (string.IsNullOrEmpty(value)) return default(T);
			return JsonConvert.DeserializeObject<T>(value);
		}

	}
}
