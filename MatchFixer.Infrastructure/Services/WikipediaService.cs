using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.Wikipedia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace MatchFixer.Infrastructure.Services
{
	public class WikipediaService : IWikipediaService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<WikipediaService> _logger;
		private readonly ITeamNameResolver _teamNameResolver;
		private readonly MatchFixerDbContext _dbContext;
		private static readonly SemaphoreSlim _wikiSemaphore = new(3, 3);


		public WikipediaService(HttpClient httpClient, ILogger<WikipediaService> logger, ITeamNameResolver teamNameResolver, MatchFixerDbContext dbContext)
		{
			_httpClient = httpClient;
			_logger = logger;
			_teamNameResolver = teamNameResolver;
			_dbContext = dbContext;

			_httpClient.BaseAddress = new Uri("https://en.wikipedia.org/w/");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MatchFixer/1.0");
		}

		private static readonly Dictionary<string, string> WikipediaTitleOverrides =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["Bodo/Glimt"] = "Bodø/Glimt",
				["Malmo FF"] = "Malmö FF"
			};

		public async Task<WikiTeamInfo?> GetTeamInfoAsync(string teamName)
		{
			try
			{

				var requestedTeamName =
					Uri.UnescapeDataString(teamName);

				if (string.IsNullOrWhiteSpace(requestedTeamName))
					return null;

				var team = await _dbContext.Teams
					.AsNoTracking()
					.Include(t => t.Aliases)
					.SingleOrDefaultAsync(t =>
						t.Name == requestedTeamName);

				if (team == null)
				{
					_logger.LogWarning(
						"Team not found in database: {TeamName}",
						requestedTeamName);

					return null;
				}


				var existing = await _dbContext.TeamWikiInfos
					.AsNoTracking()
					.SingleOrDefaultAsync(x =>
						x.TeamId== team.Id);


				if (existing != null)
				{
					return new WikiTeamInfo
					{
						Name = team.Name,

						Summary = existing!.Summary,

						ImageUrl = existing.ImageUrl,

						WikipediaUrl = existing.WikipediaUrl
					};
				}

				var wikipediaAlias =
					team.Aliases
						.FirstOrDefault()
						?.Alias
						?? team.Name;

				var pageTitle = NormalizeTeamName(wikipediaAlias);
				var wikiData = await FetchFromWikipediaAsync(pageTitle);

				if (wikiData == null)
				{
					_logger.LogWarning(
						"Wikipedia returned no data for {TeamName}",
						pageTitle);

					return null;
				}

				wikiData.Name = team.Name;

				var entity = existing ?? new TeamWikiInfo
				{
					Id = Guid.NewGuid(),

					TeamId = team.Id
				};

				entity.Summary = wikiData.Summary;

				entity.ImageUrl = wikiData.ImageUrl;

				entity.WikipediaUrl = wikiData.WikipediaUrl;

				entity.LastUpdatedUtc = DateTime.UtcNow;


				if (existing == null)
				{
					_dbContext.TeamWikiInfos.Add(entity);
				}

				await _dbContext.SaveChangesAsync();

				return wikiData;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error fetching Wikipedia info for {TeamName}",
					teamName);

				return null;
			}
		}

		private async Task<WikiTeamInfo?> FetchFromWikipediaAsync(string pageTitle)
		{
			await _wikiSemaphore.WaitAsync();

			try
			{
				// Small pacing delay
				await Task.Delay(250);

				// Get summary via REST API
				var summary =
					await GetSummaryAsync(pageTitle)
					?? "No summary available.";

				// Query Wikipedia API for images
				var encodedTitle = Uri.EscapeDataString(pageTitle);

				var url =
					$"api.php?action=query" +
					$"&prop=pageimages|images" +
					$"&piprop=thumbnail|original" +
					$"&pithumbsize=300" +
					$"&imlimit=50" +
					$"&format=json" +
					$"&titles={encodedTitle}";

				var response = await SendWikiRequestAsync(url);

				response.EnsureSuccessStatusCode();

				var jsonDoc =
					JsonDocument.Parse(
						await response.Content.ReadAsStringAsync());

				var pages =
					jsonDoc.RootElement
						.GetProperty("query")
						.GetProperty("pages");

				var page = pages.EnumerateObject().First();

				if (page.Name == "-1")
					return null;

				var imageUrl =
					await GetTeamLogoAsync(page.Value, pageTitle);

				return new WikiTeamInfo
				{
					Name = pageTitle,

					Summary = summary,

					ImageUrl = imageUrl,

					WikipediaUrl =
						$"https://en.wikipedia.org/wiki/{pageTitle.Replace(" ", "_")}"
				};
			}
			finally
			{
				_wikiSemaphore.Release();
			}
		}


		private string NormalizeTeamName(string teamName)
		{
			if (string.IsNullOrWhiteSpace(teamName))
				return teamName;

			var cleaned = teamName.Trim();
			cleaned = Regex.Replace(cleaned, @"\s+", " ");
			cleaned = cleaned.Normalize(NormalizationForm.FormC);

			if (WikipediaTitleOverrides.TryGetValue(cleaned, out var wikiTitle))
				return wikiTitle;

			return cleaned;
		}

		private async Task<string?> GetSummaryAsync(string pageTitle)
		{
			try
			{
				var encoded = Uri.EscapeDataString(pageTitle);
				var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{encoded}";

				var response = await SendWikiRequestAsync(url);
				if (!response.IsSuccessStatusCode)
					return null;

				var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

				return json.RootElement.TryGetProperty("extract", out var extract)
					? extract.GetString()
					: null;
			}
			catch
			{
				return null;
			}
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
							// Priority order: crest > logo > badge > emblem > flag
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

				var response = await SendWikiRequestAsync(url);
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

				var response = await SendWikiRequestAsync(url);
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
				@"\|\s*logo\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*crest\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*badge\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*image\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*logo_image\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*logo_black\s*=\s*\[\[\s*(?:File|Image):([^|\]]+)",
				@"\|\s*logo\s*=\s*([^\|\r\n\]]+)",
				@"\|\s*crest\s*=\s*([^\|\r\n\]]+)",
				@"\|\s*badge\s*=\s*([^\|\r\n\]]+)",
				@"\|\s*image\s*=\s*([^\|\r\n\]]+)",
				@"\|\s*logo_image\s*=\s*([^\|\r\n\]]+)",
				@"\|\s*logo_black\s*=\s*([^\|\r\n\]]+)"
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

				var response = await SendWikiRequestAsync(url);
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

		private async Task<HttpResponseMessage> SendWikiRequestAsync(string url)
		{
			// Small pacing between ALL wiki requests
			await Task.Delay(150);

			var response = await _httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();

			return response;
		}

	}
}
