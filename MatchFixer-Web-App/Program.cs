using MatchFixer_Web_App.Areas.Admin.Hubs;
using MatchFixer.Common.Identity;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.Middlewares;
using MatchFixer.Core.Services;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Services;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.Services;
using MatchFixer_Web_App.Areas.Admin.Services.MatchFixer_Web_App.Areas.Admin.Services;
using MatchFixer_Web_App.Hubs;
using MatchFixer.Core.Services.Admin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<MatchFixerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Configuration.AddUserSecrets<Program>();								// Add User Secrets
builder.Services.AddHttpClient();                                               // Add HTTP Client


builder.Services.AddHttpClient<IFootballApiService, FootballApiService>();				// Add the FootballAPI Service ( newly added interface here )
builder.Services.AddScoped<IUpcomingMatchSeederService, UpcomingMatchSeederService>();  // Upcoming Matches Seeder Service

builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();			// Add the Admin Dashboard Service
builder.Services.AddScoped<IAdminUserService, AdminUserService>();						// Add the Admin User Service
builder.Services.AddScoped<IAdminWalletService, AdminWalletService>();					// Add the Admin Wallet Service
builder.Services.AddScoped<IAdminEmailService, AdminEmailService>();				    // Add the Admin Email Service
builder.Services.AddScoped<IAdminUserBetsService, AdminUserBetsService>();				// Add the Admin User Bets Service
builder.Services.AddScoped<IAdminTeamsService, AdminTeamsService>();					// Add the Admin Teams Service
builder.Services.AddScoped<IAdminBetInsightsService, AdminBetInsightsService>();        // Add the Admin BetInsights Service
builder.Services.AddScoped<IAdminEventsService, AdminEventService>();					// Add the Admin Events Service ( historical for call events ) 
builder.Services.AddScoped<IAdminTrophyService, AdminTrophyService>();					// Add the Admin Trophy Service 

builder.Services.AddHttpClient<WikipediaService>();                             // Add the Wikipedia Service ( HTTP Client ) 
builder.Services.AddScoped<IWikipediaService, WikipediaService>();              // Add the Wikipedia Service 
builder.Services.AddHostedService<UserCleanupService>();						// Add User Cleanup Service ( background service ) 
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
builder.Services.AddScoped<IMatchEventNotifier, MatchEventNotifier>();          // Add the Match Event Notifier  	
builder.Services.AddScoped<IMatchGuessGameService, MatchGuessGameService>();	// Add the Match Guess Game Service 
builder.Services.AddScoped<ILogoQuizService, LogoQuizService>();                // Add the Logo Quiz Service
builder.Services.AddScoped<IMatchFixScoreService, MatchFixScoreService>();		// Add the MatchFix Score Service 
builder.Services.AddScoped<IBettingService, BettingService>();                  // Add the Betting Service 
builder.Services.AddScoped<ILiveMatchResultService, LiveMatchResultService>();  // Add the Live Match Result Service 
builder.Services.AddScoped<IEventsResultsService, EventsResultsService>();      // Add the Events Results Service




builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;
	})
	.AddEntityFrameworkStores<MatchFixerDbContext>()
	.AddDefaultTokenProviders();

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
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SignalR (single registration, with detailed errors)
builder.Services.AddSignalR(o =>
{
	o.EnableDetailedErrors = true;
	o.KeepAliveInterval = TimeSpan.FromSeconds(15);
	o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// CORS (for SignalR)
builder.Services.AddCors(o =>
{
	o.AddPolicy("SignalRCors", p => p
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials()
		.SetIsOriginAllowed(_ => true));
});

// Admin Insights Notifier 
builder.Services.AddScoped<IAdminInsightsNotifier, AdminInsightsNotifier>();

builder.Services.AddScoped<IBoostQueryService, BoostQueryService>();

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", p => p.RequireRole(Roles.Admin));

	// One policy per permission
	options.AddPolicy(Permissions.ManageUsers, p => p.RequireClaim("permission", Permissions.ManageUsers));
	options.AddPolicy(Permissions.ManageWallets, p => p.RequireClaim("permission", Permissions.ManageWallets));
	options.AddPolicy(Permissions.ManageMatchEvents, p => p.RequireClaim("permission", Permissions.ManageMatchEvents));
});

var app = builder.Build();


app.UseResponseCaching();

using (var scope = app.Services.CreateScope())
{

	var services = scope.ServiceProvider;
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

	// trophies
	await SeedAllTrophiesAsync(services);								  // Seed all trophies 
	// profile pictures
	await SeedDefaultProfilePicture(userManager, services);             // Seed the Default User Image
	// deleted user's profile pictures
	await SeedDeletedUsersProfilePicture(userManager, services);       // Seed the Deleted User Image ( when user deletes their profile ) 
	// teams 
	await SeedTeams(services);                                       // Seed the Teams in the Teams Table
	// upcoming events 
	await SeedUpcomingMatchEventsAsync(services);                   // UpcomingMatchEvent (API)
	// historical match results
	await SeedMatchResultsAsync(services);                        // Seed the Match Results for the 2023 seasons in the Premier League, LaLiga, Bundesliga and Serie A
	// seed the Admin & Moderator Roles
	await SeedRolesAndAdminAsync(services);						// Seed the Roles ( Admin and Moderator ) and the Admin account

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.MapHub<MatchEventHub>("/matchEventHub");
app.MapHub<AdminInsightsHub>("/hubs/admin-insights");

app.UseCors("SignalRCors");

app.UseAuthentication();
app.UseAuthorization();

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
