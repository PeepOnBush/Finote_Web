using Finote_API.Services.SendEmail;
using Finote_Web.Models.Data;
using Finote_Web.Permissions;
using Finote_Web.Repositories.Logging;
using Finote_Web.Repositories.Overview;
using Finote_Web.Repositories.Permissions;
using Finote_Web.Repositories.Transactions;
using Finote_Web.Repositories.UserRepo;
using Finote_Web.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Finote_Web.Repositories.Charts;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure DbContext
builder.Services.AddDbContext<FinoteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddHttpClient();

// 2. Configure Identity Services
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    // You can configure password requirements, etc. here if you want
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<FinoteDbContext>()
    .AddDefaultTokenProviders();

// ===== DEFINE AUTHORIZATION POLICIES =====
builder.Services.AddAuthorization(options =>
{
    // Policy for the Overview page
    options.AddPolicy("CanViewOverview", policy =>
        policy.RequireClaim(AppPermissions.ViewOverview, "true"));

    // Policy for the Statistics page
    options.AddPolicy("CanViewStatistics", policy =>
        policy.RequireClaim(AppPermissions.ViewStatistics, "true"));

    // Policy for the Account Management page
    options.AddPolicy("CanAccessAccountManagement", policy =>
        policy.RequireClaim(AppPermissions.AccessAccountManagement, "true"));

    // Policy for the Transaction Management page
    options.AddPolicy("CanAccessTransactionManagement", policy =>
        policy.RequireClaim(AppPermissions.AccessTransactionManagement, "true"));
});
// 3. ===== CONFIGURE THE AUTHENTICATION COOKIE =====
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // ===== ADD THESE LINES =====
    // This forces the session to expire if the user is inactive for 20 minutes
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);

    // This resets the timer if they are active
    options.SlidingExpiration = true;

    // This tells the browser: "This cookie is strictly for this session only"
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // ===========================
});
// ====================================================================

// 4. Register Repositories
builder.Services.AddScoped<IOverviewRepository, OverviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPermissionsRepository, PermissionsRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IChartRepository, ChartRepository>();

// 5. Add MVC services
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Seed the default roles ("Admin", "User", etc.) on startup
await app.SeedRolesAsync();
await app.SeedApiKeysAsync();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. ===== ACTIVATE AUTHENTICATION & AUTHORIZATION =====
// Routing -> Authentication -> Authorization
app.UseAuthentication(); // Checks who the user is by reading the cookie
app.UseAuthorization();  // Checks if the authenticated user is allowed to access the resource
// =======================================================


// 7. Set the default route back to the dashboard. The system will handle redirects.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();