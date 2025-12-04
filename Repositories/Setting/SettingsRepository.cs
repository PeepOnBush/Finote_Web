using Finote_Web.Models.Data;
using Microsoft.EntityFrameworkCore;
using System; 
using System.IO; 
public class SettingsRepository : ISettingsRepository
{
    private readonly FinoteDbContext _context;
    public SettingsRepository(FinoteDbContext context) { _context = context; }

    public async Task<string> GetApiKeyAsync(string keyName)
    {
        var apiKey = await _context.ApiKeys.FirstOrDefaultAsync(k => k.KeyName == keyName);
        return apiKey?.KeyValue ?? string.Empty;
    }

    public async Task UpdateApiKeyAsync(string keyName, string newKeyValue, string updatedById)
    {
        var apiKey = await _context.ApiKeys.FirstOrDefaultAsync(k => k.KeyName == keyName);
        if (apiKey != null)
        {
            apiKey.KeyValue = newKeyValue;
            // Optionally, you could update a "LastUpdatedAt" field here too
            await _context.SaveChangesAsync();
        }
        else // If the key doesn't exist, create it.
        {
            _context.ApiKeys.Add(new ApiKey
            {
                KeyName = keyName,
                KeyValue = newKeyValue,
                CreatedAt = DateTime.UtcNow,
                WhoCreatedId = updatedById
            });
            await _context.SaveChangesAsync();
        }
    }

    // ===== NEW METHOD FOR SOFT DELETING =====
    public async Task DeleteApiKeyAsync(string keyName, string deletedById)
    {
        var apiKey = await _context.ApiKeys.FirstOrDefaultAsync(k => k.KeyName == keyName);
        if (apiKey != null)
        {
            apiKey.KeyValue = string.Empty; // Set the key value to empty
            apiKey.DeletedAt = DateTime.UtcNow;
            apiKey.WhoDeletedId = deletedById;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<string> BackupDatabaseAsync()
    {
        // 1. Get the database name and connection from EF Core
        var dbConnection = _context.Database.GetDbConnection();
        var dbName = dbConnection.Database;

        // 2. Define the backup file path
        var backupFileName = $"{dbName}-backup-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak";

        // IMPORTANT: This path is relative to the web application's root.
        // Ensure the SQL Server service has write permissions to this directory.
        var backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups", backupFileName);

        // 3. Create and execute the T-SQL backup command
        var backupCommand = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupFilePath}' WITH NOFORMAT, INIT, NAME = N'{dbName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

        await _context.Database.ExecuteSqlRawAsync(backupCommand);

        // 4. Return the full path of the created file
        return backupFilePath;
    }
}