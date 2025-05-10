using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Identity.UI.Services;

using MatchFixer.Core.Contracts;
using MatchFixer.Core.Services;
using MatchFixer.Core.Middlewares;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Services;

using static MatchFixer.Infrastructure.SeedData.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MatchFixerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Configuration.AddUserSecrets<Program>();								// Add User Secrets
builder.Services.AddHttpClient();												// Add HTTP Client
builder.Services.AddHostedService<UserCleanupService>();						// Add User Cleanup Service ( background service ) 
builder.Services.AddScoped<ITimezoneService, TimezoneService>();				// Add the Timezone Service ( NodaTime )
builder.Services.AddHttpContextAccessor();                                      // Add HTTP Context Accessor
builder.Services.AddScoped<ISessionService, SessionService>();					// Add the Session Service ( currently used mainly for the MatchFix Guessing Game ) 
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();			// Add URL Helper Factory
builder.Services.AddTransient<IEmailSender, EmailSender>();						// Register Email Sender Service 
builder.Services.AddScoped<IUserService, UserService>();						// Add the User Service 
builder.Services.AddScoped<IImageService, ImageService>();						// Add the Image Service 
builder.Services.AddScoped<IProfileService, ProfileService>();					// Add the Profile Service 
builder.Services.AddScoped<IMatchGuessGameService, MatchGuessGameService>();	// Add the Match Guess Game Service 
builder.Services.AddHttpClient<FootballApiService>();							// Add the FootballAPI Service 
builder.Services.AddScoped<IMatchFixScoreService, MatchFixScoreService>();		// Add the MatchFix Score Service 



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
	options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout 
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{

	var services = scope.ServiceProvider;
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

	await SeedDefaultProfilePicture(userManager, services);          // Seed the Default User Image
	await SeedMatchResultsAsync(services);							// Seed the Match Results for the 2023 seasons in the Premier League, LaLiga, Bundesliga

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

app.UseAuthentication();
app.UseAuthorization();

// custom middleware for initializing the session 
app.UseMiddleware<SessionInitializationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Game}/{action=Landing}/{id?}");

app.MapRazorPages();

app.Run();
