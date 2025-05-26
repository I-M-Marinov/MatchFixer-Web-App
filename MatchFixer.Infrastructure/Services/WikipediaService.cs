using System.Text.Json;
using System.Web;
using MatchFixer.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using MatchFixer.Infrastructure.Models.Wikipedia;

namespace MatchFixer.Infrastructure.Services
{
	public class WikipediaService : IWikipediaService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<WikipediaService> _logger;

		public WikipediaService(HttpClient httpClient, ILogger<WikipediaService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;

			_httpClient.BaseAddress = new Uri("https://en.wikipedia.org/w/");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MatchFixer/1.0");
		}

		public async Task<WikiTeamInfo?> GetTeamInfoAsync(string teamName)
		{
			try
			{
				var cleanedName = CleanTeamName(teamName);

				// Clean the team name before querying
				var encodedTeamName = HttpUtility.UrlEncode(cleanedName);

				// Get basic info with better image properties
				var url = $"api.php?action=query&prop=extracts|pageimages|images&exintro=true" +
						 $"&explaintext=true&piprop=thumbnail|original&pithumbsize=300" +
						 $"&imlimit=50&format=json&titles={encodedTeamName}";

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var jsonDoc = JsonDocument.Parse(content);
				var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");
				var page = pages.EnumerateObject().First();

				if (page.Name == "-1") return null;


				var pageTitle = page.Value.GetProperty("title").GetString() ?? cleanedName;

				// Get team logo/crest
				var imageUrl = await GetTeamLogoAsync(page.Value, pageTitle);

				// Get players list
				var players = await GetTeamPlayersAsync(pageTitle);

				return new WikiTeamInfo
				{
					Name = pageTitle,
					Summary = page.Value.GetProperty("extract").GetString() ?? "No summary available",
					ImageUrl = imageUrl,
					Players = players,
					WikipediaUrl = $"https://en.wikipedia.org/wiki/{pageTitle.Replace(" ", "_")}"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error fetching Wikipedia info for {teamName}");
				return null;
			}
		}

		private string CleanTeamName(string teamName)
		{
			if (string.IsNullOrWhiteSpace(teamName))
				return teamName;

			// Clean up the Team name
			var cleaned = teamName.Trim();

			// Remove extra spaces
			 cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");

			return cleaned;
		}

		private async Task<string?> GetTeamLogoAsync(JsonElement pageElement, string pageTitle)
		{
			try
			{
				// Get the page wikitext to find the infobox logo
				var logoFromInfobox = await GetLogoFromInfoboxAsync(pageTitle);
				if (!string.IsNullOrEmpty(logoFromInfobox))
					return logoFromInfobox;

				// Look through all images for logo/crest 
				if (pageElement.TryGetProperty("images", out var images))
				{
					var logoImages = new List<string>();

					foreach (var image in images.EnumerateArray())
					{
						var imageName = image.GetProperty("title").GetString()?.ToLower();
						if (imageName != null)
						{
							// Priority order: crest > logo > badge > emblem
							if (imageName.Contains("crest"))
								logoImages.Insert(0, image.GetProperty("title").GetString());
							else if (imageName.Contains("logo"))
								logoImages.Add(image.GetProperty("title").GetString());
							else if (imageName.Contains("badge") || imageName.Contains("emblem"))
								logoImages.Add(image.GetProperty("title").GetString());
						}
					}

					// Try each logo image
					foreach (var logoImage in logoImages)
					{
						var imageUrl = await GetImageUrlAsync(logoImage);
						if (!string.IsNullOrEmpty(imageUrl))
							return imageUrl;
					}
				}

				return await SearchForTeamLogoAsync(pageTitle);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error getting team logo for {PageTitle}", pageTitle);
				return null;
			}
		}

		private async Task<string?> GetImageUrlAsync(string imageTitle)
		{
			try
			{
				var encodedImageTitle = HttpUtility.UrlEncode(imageTitle);
				var url = $"api.php?action=query&titles={encodedImageTitle}&prop=imageinfo" +
						 $"&iiprop=url&iiurlwidth=300&format=json";

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var jsonDoc = JsonDocument.Parse(content);

				var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");
				var page = pages.EnumerateObject().First();

				if (page.Value.TryGetProperty("imageinfo", out var imageInfo) &&
					imageInfo.GetArrayLength() > 0)
				{
					return imageInfo[0].GetProperty("thumburl").GetString();
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		private async Task<string?> GetLogoFromInfoboxAsync(string pageTitle)
		{
			try
			{
				var encodedPageTitle = HttpUtility.UrlEncode(pageTitle);

				// Get the page wikitext
				var url = $"api.php?action=query&titles={encodedPageTitle}&prop=revisions" +
						 $"&rvprop=content&format=json&formatversion=2";

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var jsonDoc = JsonDocument.Parse(content);

				if (jsonDoc.RootElement.TryGetProperty("query", out var query) &&
					query.TryGetProperty("pages", out var pages) &&
					pages.GetArrayLength() > 0)
				{
					var page = pages[0];
					if (page.TryGetProperty("revisions", out var revisions) &&
						revisions.GetArrayLength() > 0)
					{
						var wikitext = revisions[0].GetProperty("content").GetString();
						if (!string.IsNullOrEmpty(wikitext))
						{
							// Look for infobox logo field
							var logoFileName = ExtractLogoFromInfobox(wikitext);
							if (!string.IsNullOrEmpty(logoFileName))
							{
								return await GetImageUrlAsync($"File:{logoFileName}");
							}
						}
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, $"Error getting logo from infobox for {pageTitle}");
				return null;
			}
		}

		private string? ExtractLogoFromInfobox(string wikitext)
		{
			// Common infobox logo field patterns
			var logoPatterns = new[]
			{
				@"\|\s*logo\s*=\s*([^\|\r\n]+)",
				@"\|\s*crest\s*=\s*([^\|\r\n]+)",
				@"\|\s*badge\s*=\s*([^\|\r\n]+)",
				@"\|\s*image\s*=\s*([^\|\r\n]+)",
				@"\|\s*logo_image\s*=\s*([^\|\r\n]+)"
			};

			foreach (var pattern in logoPatterns)
			{
				var match = System.Text.RegularExpressions.Regex.Match(wikitext, pattern,
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				if (match.Success)
				{
					var logoValue = match.Groups[1].Value.Trim();

					// Clean up the filename
					logoValue = logoValue.Replace("[[", "").Replace("]]", "");
					if (logoValue.Contains("|"))
						logoValue = logoValue.Split('|')[0];

					// Skip if it's clearly not an image
					if (logoValue.ToLower().EndsWith(".svg") ||
						logoValue.ToLower().EndsWith(".png") ||
						logoValue.ToLower().EndsWith(".jpg") ||
						logoValue.ToLower().EndsWith(".jpeg") ||
						logoValue.ToLower().Contains(".crest") ||
						logoValue.ToLower().Contains(".logo") ||
						logoValue.ToLower().Contains("crest") ||
						logoValue.ToLower().Contains("logo"))
					{
						return logoValue;
					}
				}
			}

			return null;
		}

		private async Task<string?> SearchForTeamLogoAsync(string teamName)
		{
			try
			{
				// Search for team logo on Commons
				var searchTerm = $"{teamName} logo";
				var encodedSearch = HttpUtility.UrlEncode(searchTerm);
				var url = $"api.php?action=query&list=search&srsearch={encodedSearch}" +
						 $"&srnamespace=6&srlimit=5&format=json";

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				var jsonDoc = JsonDocument.Parse(content);

				if (jsonDoc.RootElement.TryGetProperty("query", out var query) &&
					query.TryGetProperty("search", out var searchResults) &&
					searchResults.GetArrayLength() > 0)
				{
					var firstResult = searchResults[0].GetProperty("title").GetString();
					return await GetImageUrlAsync(firstResult);
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		private async Task<List<string>> GetTeamPlayersAsync(string teamName)
		{
			try
			{
				var encodedTeamName = HttpUtility.UrlEncode(teamName);

				// Get page sections to find squad/players section
				var sectionsUrl = $"api.php?action=parse&page={encodedTeamName}&prop=sections&format=json";
				var sectionsResponse = await _httpClient.GetAsync(sectionsUrl);
				sectionsResponse.EnsureSuccessStatusCode();
				var sectionsContent = await sectionsResponse.Content.ReadAsStringAsync();
				var sectionsDoc = JsonDocument.Parse(sectionsContent);

				// Find squad/players section with safer JSON parsing
				int? squadSectionIndex = null;
				if (sectionsDoc.RootElement.TryGetProperty("parse", out var parse) &&
					parse.TryGetProperty("sections", out var sections) &&
					sections.ValueKind == JsonValueKind.Array)
				{
					foreach (var section in sections.EnumerateArray())
					{
						// Get section name
						var sectionName = section.TryGetProperty("line", out var lineProp)
							? lineProp.GetString()?.ToLower()
							: null;

						if (sectionName != null && (sectionName.Contains("squad") ||
												  sectionName.Contains("players") ||
												  sectionName.Contains("current") ||
												  sectionName.Contains("roster")))
						{
							// Get section index
							if (section.TryGetProperty("index", out var indexProp))
							{
								if (indexProp.ValueKind == JsonValueKind.Number)
								{
									squadSectionIndex = indexProp.GetInt32();
								}
								else if (indexProp.ValueKind == JsonValueKind.String &&
										 int.TryParse(indexProp.GetString(), out var stringIndex))
								{
									squadSectionIndex = stringIndex;
								}
							}
							break;
						}
					}
				}

				if (!squadSectionIndex.HasValue)
				{
					_logger.LogInformation($"No squad section found for {teamName}");
					return new List<string>();
				}

				// Get the squad section content
				var squadUrl = $"api.php?action=parse&page={encodedTeamName}&section={squadSectionIndex}" +
							  $"&prop=wikitext&format=json";
				var squadResponse = await _httpClient.GetAsync(squadUrl);
				squadResponse.EnsureSuccessStatusCode();
				var squadContent = await squadResponse.Content.ReadAsStringAsync();
				var squadDoc = JsonDocument.Parse(squadContent);

				if (squadDoc.RootElement.TryGetProperty("parse", out var squadParse) &&
					squadParse.TryGetProperty("wikitext", out var wikitext) &&
					wikitext.TryGetProperty("*", out var wikitextContent))
				{
					var content = wikitextContent.GetString();
					return !string.IsNullOrEmpty(content)
						? ParsePlayersFromWikitext(content)
						: new List<string>();
				}

				return new List<string>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting players for {TeamName}", teamName);
				return new List<string>();
			}
		}

		private List<string> ParsePlayersFromWikitext(string wikitext)
		{
			var players = new List<string>();

			if (string.IsNullOrEmpty(wikitext))
				return players;

			// Look for player links in the format [[Player Name]]

			var playerPattern = @"\[\[([^|\]]+)(?:\|[^\]]+)?\]\]";
			var matches = System.Text.RegularExpressions.Regex.Matches(wikitext, playerPattern);

			foreach (System.Text.RegularExpressions.Match match in matches)
			{
				var playerName = match.Groups[1].Value.Trim();

				// Filter out obvious non-player entries
				if (!string.IsNullOrEmpty(playerName) &&
					!playerName.ToLower().Contains("category:") &&
					!playerName.ToLower().Contains("file:") &&
					!playerName.ToLower().Contains("image:") &&
					!playerName.Contains("flag") &&
					playerName.Length > 2 &&
					playerName.Length < 50) // Name max length
				{
					players.Add(playerName);
				}
			}

			// Remove duplicates and return
			return players.Distinct().ToList();
		}
	}
}
