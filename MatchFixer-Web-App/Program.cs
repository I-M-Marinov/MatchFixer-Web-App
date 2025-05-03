
using MatchFixer.Core.Contracts;
using MatchFixer.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Routing;

using static MatchFixer.Infrastructure.SeedData.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MatchFixerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddHttpClient(); // Add HTTP Client
builder.Services.AddHostedService<UserCleanupService>();
builder.Services.AddScoped<ITimezoneService, TimezoneService>(); // Add the Timezone Service ( NodaTime )
builder.Services.AddHttpContextAccessor(); // Add HTTP Context Accessor
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>(); // Add URL Helper Factory
builder.Services.AddTransient<IEmailSender, EmailSender>(); // Register Email Sender Service 
builder.Services.AddScoped<IUserService, UserService>(); // Add the User Service 
builder.Services.AddScoped<IImageService, ImageService>();  // Add the Image Service 
builder.Services.AddScoped<IProfileService, ProfileService>(); // Add the Profile Service 
builder.Services.AddHttpClient<FootballApiService>(); // Add the FootballAPI Service 


builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;
	})
	.AddEntityFrameworkStores<MatchFixerDbContext>()
	.AddDefaultTokenProviders();

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
