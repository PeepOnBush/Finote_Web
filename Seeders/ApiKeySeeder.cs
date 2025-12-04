using Finote_Web.Models.Data;
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;

public static class ApiKeySeeder
{
    public static async Task SeedApiKeysAsync(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<FinoteDbContext>();
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<Users>>(); // Get UserManager

            string defaultKeyName = "DefaultApiKey";
            var apiKeyExists = await context.ApiKeys.AnyAsync(k => k.KeyName == defaultKeyName);

            if (!apiKeyExists)
            {
                // Find the first user with the "Admin" role to be the creator
                var adminUser = (await userManager.GetUsersInRoleAsync("Admin")).FirstOrDefault();

                string creatorId;
                if (adminUser != null)
                {
                    creatorId = adminUser.Id;
                }
                else
                {
                    // Fallback: If no admin exists yet, you must have a plan.
                    // This could be a hardcoded system user ID or an error.
                    // For now, let's throw an exception to make it obvious.
                    throw new InvalidOperationException("Cannot seed API Key: No Admin user found to assign as creator.");
                }

                context.ApiKeys.Add(new ApiKey
                {
                    KeyName = defaultKeyName,
                    KeyValue = Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.UtcNow,
                    WhoCreatedId = creatorId // <-- THIS IS THE FIX
                });
                await context.SaveChangesAsync();
                Console.WriteLine($"Default API Key '{defaultKeyName}' created.");
            }
        }
    }
}