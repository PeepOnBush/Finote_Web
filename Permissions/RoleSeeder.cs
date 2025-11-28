using Microsoft.AspNetCore.Identity;

namespace Finote_Web.Permissions
{
    public static class RoleSeeder
    {
        // This is an extension method to make calling it from Program.cs cleaner
        public static async Task SeedRolesAsync(this IApplicationBuilder app)
        {
            // We need a 'scope' to get the services we need, like RoleManager
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Define the roles you want to exist in your application
                string[] roleNames = { "Admin", "Editor", "User" };

                foreach (var roleName in roleNames)
                {
                    // Check if the role already exists
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        // If it doesn't exist, create it
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        Console.WriteLine($"Role '{roleName}' created successfully.");
                    }
                }
            }
        }
    }
}