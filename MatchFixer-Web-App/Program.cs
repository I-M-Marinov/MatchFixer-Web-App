using MatchFixer.Common.Identity;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.Middlewares;
using MatchFixer.Core.Services;
using MatchFixer.Core.Services.Admin;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Services;
using MatchFixer_Web_App.Areas.Admin.Hubs;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.Services;
using MatchFixer_Web_App.Areas.Admin.Services.MatchFixer_Web_App.Areas.Admin.Services;
using MatchFixer_Web_App.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using static MatchFixer.Infrastructure.SeedData.SeedData;


var builder = WebApplication.CreateBuilder(args);

// Configuration for the password of the application 
builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 8;
	options.Password.RequiredUniqueChars = 1;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.Configure<FormOptions>(options =>
{
	options.ValueCountLimit = 5000; // Increase from default 1024
});

builder.Services.AddDbContext<MatchFixerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMemoryCache();

// Behind the Synology reverse proxy: trust its forwarded headers so the real
// client IP + https scheme are seen (needed for secure cookies, rate limiting,
// logging). Only valid because the app port is reachable only via the proxy.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
	o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	o.KnownNetworks.Clear();
	o.KnownProxies.Clear();
});

// Throttle auth endpoints per client IP to blunt brute force / credential stuffing.
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
	{
		var path = httpContext.Request.Path.Value ?? string.Empty;
		var isAuth =
			path.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
			path.Contains("/Account/Register", StringComparison.OrdinalIgnoreCase) ||
			path.Contains("/Account/ForgotPassword", StringComparison.OrdinalIgnoreCase) ||
			path.Contains("/Account/ResendEmailConfirmation", StringComparison.OrdinalIgnoreCase) ||
			path.Contains("/Account/ResetPassword", StringComparison.OrdinalIgnoreCase);

		if (!isAuth)
			return RateLimitPartition.GetNoLimiter("open");

		var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		return RateLimitPartition.GetFixedWindowLimiter("auth:" + key, _ => new FixedWindowRateLimiterOptions
		{
			PermitLimit = 10,
			Window = TimeSpan.FromMinutes(1),
			QueueLimit = 0
		});
	});
});


builder.Configuration.AddUserSecrets<Program>();								// Add User Secrets
builder.Services.AddHttpClient();                                               // Add HTTP Client


builder.Services.AddHttpClient<IFootballApiService, FootballApiService>();				// Add the FootballAPI Service ( newly added interface here )
builder.Services.AddScoped<IUpcomingMatchSeederService, UpcomingMatchSeederService>();  // Upcoming Matches Seeder Service
builder.Services.AddScoped<ITeamLogoSyncService, TeamLogoSyncService>();                // Team Logo Sync Service

builder.Services.AddScoped<IHomePageService, HomePageService>();						// Home Page Service ( new showing last big wins with 3 legs or more ) 
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();			// Add the Admin Dashboard Service
builder.Services.AddScoped<IAdminUserService, AdminUserService>();						// Add the Admin User Service
builder.Services.AddScoped<IAdminWalletService, AdminWalletService>();					// Add the Admin Wallet Service
builder.Services.AddScoped<IAdminEmailService, AdminEmailService>();				    // Add the Admin Email Service
builder.Services.AddScoped<IAdminUserBetsService, AdminUserBetsService>();				// Add the Admin User Bets Service
builder.Services.AddHttpClient<IAdminTeamsService, AdminTeamsService>();				// Add the Admin Teams Service
builder.Services.AddScoped<IAdminBetInsightsService, AdminBetInsightsService>();        // Add the Admin BetInsights Service
builder.Services.AddScoped<IAdminEventsService, AdminEventService>();					// Add the Admin Events Service ( historical for call events ) 
builder.Services.AddScoped<IAdminTrophyService, AdminTrophyService>();					// Add the Admin Trophy Service 

builder.Services.AddHttpClient<WikipediaService>();                             // Add the Wikipedia Service ( HTTP Client ) 
builder.Services.AddScoped<IWikipediaService, WikipediaService>();              // Add the Wikipedia Service 
builder.Services.AddHostedService<UserCleanupService>();                        // Add User Cleanup Service ( background service ) 
builder.Services.AddHostedService<ExpiredBoostCleanupService>();                // Add the Expired Boosts Cleanup Service ( background service ) 
builder.Services.AddHostedService<BirthdayEmailService>();						// Add User Birthday Email Service ( background service that runs once a day ! ) 
builder.Services.AddScoped<ITimezoneService, TimezoneService>();				// Add the Timezone Service ( NodaTime )
builder.Services.AddHttpContextAccessor();                                      // Add HTTP Context Accessor
builder.Services.AddScoped<IUserContextService, UserContextService>();			// Add User Context Service 
builder.Services.AddScoped<ISessionService, SessionService>();					// Add the Session Service 
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();			// Add URL Helper Factory
builder.Services.AddTransient<IEmailSender, EmailSender>();						// Register Email Sender Service 
builder.Services.AddScoped<IUserService, UserService>();                        // Add the User Service
builder.Services.AddScoped<ITrophyService, TrophyService>();                    // Add the Trophy Service
builder.Services.AddScoped<IImageService, ImageService>();						// Add the Image Service 
builder.Services.AddScoped<IProfileService, ProfileService>();					// Add the Profile Service 
builder.Services.AddScoped<IWalletService, WalletService>();					// Add the Wallet Service
builder.Services.AddScoped<IOddsBoostService, OddsBoostService>();				// Add the Odds Boost Service
builder.Services.AddScoped<IMatchEventService, MatchEventService>();			// Add the Match Event Service 
builder.Services.AddScoped<IUpcomingMatchService, UpcomingMatchService>();		// Add the Upcoming Matches Service 
builder.Services.AddScoped<IWorldCupService, WorldCupService>();		// Add the World Cup Service 
builder.Services.AddScoped<IMatchEventNotifier, MatchEventNotifier>();          // Add the Match Event Notifier  	
builder.Services.AddScoped<INotificationService, NotificationService>();        // Add the Favorite Teams User Notification Service   	
builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();            // Add the Email Queue (approval) Service
builder.Services.AddHostedService<EmailDispatchService>();                      // Add the Email Dispatch background Service
builder.Services.AddScoped<IMatchGuessGameService, MatchGuessGameService>();	// Add the Match Guess Game Service
builder.Services.AddScoped<IOddsGeneratorService, OddsGeneratorService>();		// Add the Odds Generator Service
builder.Services.AddScoped<ILogoQuizService, LogoQuizService>();                // Add the Logo Quiz Service
builder.Services.AddScoped<IMatchFixScoreService, MatchFixScoreService>();		// Add the MatchFix Score Service 
builder.Services.AddScoped<IBettingService, BettingService>();                  // Add the Betting Service 
builder.Services.AddScoped<ILiveMatchResultService, LiveMatchResultService>();  // Add the Live Match Result Service 
builder.Services.AddScoped<IAutoScoringService, AutoScoringService>();     // Add the Auto Scoring Service ( API-driven result settlement )
builder.Services.AddScoped<ITournamentService, TournamentService>();            // Add the Tournament Service
builder.Services.AddHostedService<TournamentLifecycleService>();               // Opens/settles tournaments on schedule ( background service )
builder.Services.AddScoped<IEventsResultsService, EventsResultsService>();      // Add the Events Results Service
builder.Services.AddHttpClient<ITheSportsDbApiService, TheSportsDbApiService>();// Add the SportsDBApi Service
builder.Services.AddScoped<ILiveMatchQueryService, LiveMatchQueryService>();	// Add the Live Match Query Service
builder.Services.AddScoped<ILeagueTableService, LeagueTableService>();			// Add the League Table Service 
builder.Services.AddScoped<ITeamNameResolver, TeamNameResolver>();				// Add the Team Name Resolver Service




builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;
	})
	.AddEntityFrameworkStores<MatchFixerDbContext>()
	.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.ExpireTimeSpan = TimeSpan.FromHours(8);
	options.SlidingExpiration = true;
	options.LoginPath = "/Identity/Account/Login";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.Events.OnSignedIn = ctx =>
	{
		var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Auth");
		log.LogInformation("User signed in: {User} from {IP}", ctx.Principal?.Identity?.Name, ctx.HttpContext.Connection.RemoteIpAddress);
		return Task.CompletedTask;
	};
});

builder.Services.AddDistributedMemoryCache(); // In-memory session store
builder.Services.AddSession(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.IdleTimeout = TimeSpan.FromMinutes(60); // session timeout 
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNamingPolicy = null; // disables camelCase conversion
});
builder.Services.AddControllersWithViews(options =>
{
	// Lock the whole Admin area behind the Admin role so no admin endpoint
	// can be reached anonymously (defence in depth over per-controller attributes).
	options.Conventions.Add(new AdminAreaAuthorizeConvention());
})
.AddRazorRuntimeCompilation();
builder.Services.AddRazorPages(options =>
{
	// Identity auth-flow pages stay anonymous; profile management requires auth.
	options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
	options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
});

// Generate all outgoing URLs in lowercase (path only; query-string values untouched).
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
	options.LowercaseUrls = true;
});

// SignalR (single registration, with detailed errors)
builder.Services.AddSignalR(o =>
{
	o.EnableDetailedErrors = builder.Environment.IsDevelopment();
	o.KeepAliveInterval = TimeSpan.FromSeconds(15);
	o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// CORS: same-origin SignalR needs NO CORS. Only allow the explicit origins in
// configuration (AllowedOrigins, comma-separated); never reflect arbitrary
// origins together with AllowCredentials.
var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "")
	.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(o =>
{
	o.AddPolicy("SignalRCors", p =>
	{
		if (allowedOrigins.Length > 0)
			p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
	});
});

// Admin Insights Notifier 
builder.Services.AddScoped<IAdminInsightsNotifier, AdminInsightsNotifier>();

builder.Services.AddScoped<IBoostQueryService, BoostQueryService>();

builder.Services.AddAuthorization(options =>
{
	// Deny by default: every endpoint needs an authenticated user unless it
	// explicitly opts out with [AllowAnonymous].
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();

	options.AddPolicy("AdminOnly", p => p.RequireRole(Roles.Admin));

	// One policy per permission
	options.AddPolicy(Permissions.ManageUsers, p => p.RequireClaim("permission", Permissions.ManageUsers));
	options.AddPolicy(Permissions.ManageWallets, p => p.RequireClaim("permission", Permissions.ManageWallets));
	options.AddPolicy(Permissions.ManageMatchEvents, p => p.RequireClaim("permission", Permissions.ManageMatchEvents));
});

// Persist Data Protection keys outside the container so login cookies and
// antiforgery tokens survive redeploys (DP_KEYS_PATH is mounted to a volume in Docker).
var dpKeysPath = builder.Configuration["DP_KEYS_PATH"];
if (!string.IsNullOrWhiteSpace(dpKeysPath))
{
	Directory.CreateDirectory(dpKeysPath);
	builder.Services.AddDataProtection()
		.PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
		.SetApplicationName("MatchFixer");
}

var app = builder.Build();

// Must run before anything that inspects the request scheme or client IP.
app.UseForwardedHeaders();

app.UseResponseCaching();

using (var scope = app.Services.CreateScope())
{

	var services = scope.ServiceProvider;

	// IMPORTANT: migrations are OFF by default so an existing, populated database
	// is never altered on startup. The app just uses the database as-is.
	// To apply pending EF migrations deliberately, set env var RUN_MIGRATIONS=true.
	if (string.Equals(app.Configuration["RUN_MIGRATIONS"], "true", StringComparison.OrdinalIgnoreCase))
	{
		await services.GetRequiredService<MatchFixerDbContext>().Database.MigrateAsync();
	}
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

	// trophies
	await SeedAllTrophiesAsync(services);								  // Seed all trophies 
	// profile pictures
	await SeedDefaultProfilePicture(userManager, services);             // Seed the Default User Image
	// deleted user's profile pictures
	await SeedDeletedUsersProfilePicture(userManager, services);       // Seed the Deleted User Image ( when user deletes their profile ) 
	// teams 
	await SeedTeams(services);                                       // Seed the Teams in the Teams Table
	// team aliases
	await SeedTeamAliasesAsync(services);						   // Seed the Teams Aliases	
	// upcoming events 
	await SeedUpcomingMatchEventsAsync(services);                // UpcomingMatchEvent (API)
	// historical match results
	await SeedMatchResultsAsync(services);                     // Seed the Match Results for the 2023 seasons in the Premier League, LaLiga, Bundesliga and Serie A
	// seed the Admin & Moderator Roles
	await SeedRolesAndAdminAsync(services);					 // Seed the Roles ( Admin and Moderator ) and the Admin account	
    // seed the World Cup Matches
	await SeedWorldCupMatchesAsync(services);			        // Seed all the World Cup 2026 matches
	// auto-refresh knockout bracket if any slot still shows TBD team names
	await RefreshKnockoutBracketIfStaleAsync(services);         // Fix stale TBD knockout slots on startup

}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Serve the app's own branded page for 404 / 403 / other status codes
// (otherwise the reverse proxy dresses a bare 404 up as its own page).
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// Baseline security headers (no CSP yet - the app relies on inline scripts/styles).
app.Use(async (ctx, next) =>
{
	ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
	ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
	ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
	await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();
app.UseRateLimiter();
app.UseCors("SignalRCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MatchEventHub>("/matchEventHub");
app.MapHub<AdminInsightsHub>("/hubs/admin-insights").RequireAuthorization("AdminOnly");

// custom middleware for handling the session  
app.UseMiddleware<SessionInitializationMiddleware>();


app.Use(async (ctx, next) =>
{         
	if (ctx.Request.Path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase))
		ctx.Response.Redirect("/Identity/Account/Login" + ctx.Request.QueryString);
	else
		await next();
});

app.MapAreaControllerRoute(
	name: "admin",
	areaName: "Admin",
	pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//	name: "default",
//	pattern: "{controller=Game}/{action=Landing}/{id?}");

app.MapRazorPages();

app.Run();

static async Task RefreshKnockoutBracketIfStaleAsync(IServiceProvider services)
{
	var db = services.GetRequiredService<MatchFixerDbContext>();

	var hasStale = await db.WorldCupMatches
		.AnyAsync(m => m.IsKnockout
			&& (m.HomeTeam.StartsWith("TBD") || m.AwayTeam.StartsWith("TBD")
			 || m.HomeTeam == string.Empty || m.AwayTeam == string.Empty));

	if (hasStale)
	{
		var worldCupService = services.GetRequiredService<IWorldCupService>();
		await worldCupService.ReclassifyAndRefreshAsync();
	}
}

// Applies an Admin-role authorization filter to every controller in the Admin area.
public sealed class AdminAreaAuthorizeConvention : IApplicationModelConvention
{
	public void Apply(ApplicationModel application)
	{
		var adminPolicy = new AuthorizationPolicyBuilder()
			.RequireRole(Roles.Admin)
			.Build();

		foreach (var controller in application.Controllers)
		{
			var ns = controller.ControllerType.Namespace ?? string.Empty;
			if (ns.Contains(".Areas.Admin."))
				controller.Filters.Add(new AuthorizeFilter(adminPolicy));
		}
	}
}
