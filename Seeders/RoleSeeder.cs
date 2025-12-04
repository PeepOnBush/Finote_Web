using Finote_Web.Permissions;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Security.Claims;

namespace Finote_Web.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // --- 1. Seed the Roles ---
                string[] roleNames = { "Admin", "Editor", "User" };
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // --- 2. Seed the Admin Role with ALL Permissions ---
                var adminRole = await roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    // Get all permission constants from our static AppPermissions class
                    var allPermissions = typeof(AppPermissions)
                        .GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Where(f => f.IsLiteral && !f.IsInitOnly)
                        .Select(f => f.GetValue(null).ToString())
                        .ToList();

                    // Get the claims the Admin role currently has
                    var currentClaims = await roleManager.GetClaimsAsync(adminRole);

                    // Add any permissions that the Admin role doesn't already have
                    foreach (var permission in allPermissions)
                    {
                        if (!currentClaims.Any(c => c.Type == permission))
                        {
                            // A Claim's value is often used for more granular control,
                            // but for simple access, "true" is standard.
                            var claim = new Claim(permission, "true");
                            await roleManager.AddClaimAsync(adminRole, claim);
                        }
                    }
                }
            }
        }
    }
}