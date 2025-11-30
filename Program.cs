using Finote_Web.Models.Data;
using Finote_Web.Permissions;
using Finote_Web.Repositories.Logging;
using Finote_Web.Repositories.Overview;
using Finote_Web.Repositories.Permissions;
using Finote_Web.Repositories.Transactions;
using Finote_Web.Repositories.UserRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Finote_Web.Permissions;
var builder = WebApplication.CreateBuilder(args);

// 1. Configure DbContext
builder.Services.AddDbContext<FinoteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2. Configure Identity Services
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    // You can configure password requirements, etc. here if you want
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<FinoteDbContext>()
    .AddDefaultTokenProviders();

// 3. ===== THIS IS THE KEY: CONFIGURE THE AUTHENTICATION COOKIE =====
builder.Services.ConfigureApplicationCookie(options =>
{
    // If a user tries to access a protected page and is not logged in,
    // they will be redirected to this path.
    options.LoginPath = "/Account/Login";

    // Path to redirect to if a user is logged in but doesn't have permission
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.SlidingExpiration = true; // The cookie lifetime resets on each request
});
// ====================================================================

// 4. Register Repositories
builder.Services.AddScoped<IOverviewRepository, OverviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPermissionsRepository, PermissionsRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

// 5. Add MVC services
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Seed the default roles ("Admin", "User", etc.) on startup
await app.SeedRolesAsync();

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