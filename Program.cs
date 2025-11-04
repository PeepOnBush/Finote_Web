// This is an example assuming ASP.NET Core Identity with default settings.
// Your file might have more services.

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
// Example for ASP.NET Core Identity
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();


// 2. *** IMPORTANT: Configure the application cookie ***
// This tells the system where to redirect users who are not logged in.
builder.Services.ConfigureApplicationCookie(options =>
{
    // The path to your login action
    options.LoginPath = "/Account/Login";
    // You can also set paths for Access Denied, Logout, etc.
    // options.AccessDeniedPath = "/Account/AccessDenied";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. *** IMPORTANT: Add Authentication and Authorization middleware ***
// Make sure they are in this order: Routing -> Authentication -> Authorization
app.UseAuthentication(); // This middleware identifies the user
app.UseAuthorization();  // This middleware checks if the user is allowed to access the resource


// This default route is now correct. It will try to go to Home/Index,
// trigger the [Authorize] attribute, and redirect to /Account/Login.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();