

namespace MatchFixer.Common.EnumExtensions
{
	public static class EnumExtensions
	{
		public static string ToDisplayName(this Enum value)
		{
			var text = value.ToString();
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;

			return System.Text.RegularExpressions.Regex.Replace(
				text,
				"([a-z])([A-Z])",
				"$1 $2"
			);
		}
	}

}
