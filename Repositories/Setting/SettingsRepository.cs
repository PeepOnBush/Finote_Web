using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System; 
using System.IO; 
public class SettingsRepository : ISettingsRepository
{
    private readonly FinoteDbContext _context;
    public SettingsRepository(FinoteDbContext context) { _context = context; }
    private const string LastBackupDateKey = "LastBackupTimestamp";

    public async Task<string> GetApiKeyAsync(string keyName)
    {
        // Fetch the latest ACTIVE key
        var apiKey = await _context.ApiKeys
            .Where(k => k.KeyName == keyName && !k.IsDeleted)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        return apiKey?.KeyValue ?? string.Empty;
    }

    public async Task UpdateApiKeyAsync(string keyName, string newKeyValue, string userId)
    {
        // 1. Find the currently active key
        var currentActiveKey = await _context.ApiKeys
            .Where(k => k.KeyName == keyName && !k.IsDeleted)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        // 2. If it exists, "Soft Delete" it
        if (currentActiveKey != null)
        {
            currentActiveKey.IsDeleted = true;
            currentActiveKey.DeletedAt = DateTime.UtcNow;
            currentActiveKey.WhoDeletedId = userId;
            // We do NOT save changes yet, we'll do it in one transaction
        }

        // 3. Create the NEW key
        var newKey = new ApiKey
        {
            KeyName = keyName,
            KeyValue = newKeyValue,
            CreatedAt = DateTime.UtcNow,
            WhoCreatedId = userId,
            IsDeleted = false
        };

        _context.ApiKeys.Add(newKey);
        await _context.SaveChangesAsync();
    }

    // ===== NEW METHOD FOR SOFT DELETING =====
    public async Task DeleteApiKeyAsync(string keyName, string deletedById)
    {
        // Soft delete the active key
        var apiKey = await _context.ApiKeys
            .Where(k => k.KeyName == keyName && !k.IsDeleted)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        if (apiKey != null)
        {
            apiKey.IsDeleted = true;
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
        await UpdateLastBackupDateAsync(); // Update the last backup date
        // 4. Return the full path of the created file
        return backupFilePath;
    }
    public async Task<DateTime?> GetLastBackupDateAsync()
    {
        var setting = await _context.SystemSettings.FindAsync(LastBackupDateKey);
        if (setting != null && DateTime.TryParse(setting.SettingValue, out DateTime date))
        {
            return date;
        }
        return null;
    }

    public async Task UpdateLastBackupDateAsync()
    {
        var setting = await _context.SystemSettings.FindAsync(LastBackupDateKey);
        var now = DateTime.UtcNow;

        if (setting != null)
        {
            // If the setting exists, update its value
            setting.SettingValue = now.ToString("o"); // "o" is the round-trip format, e.g., "2025-12-05T10:30:00.12345Z"
        }
        else
        {
            // If it's the first backup ever, create the setting
            _context.SystemSettings.Add(new SystemSetting
            {
                SettingKey = LastBackupDateKey,
                SettingValue = now.ToString("o")
            });
        }
        await _context.SaveChangesAsync();
    }
    public async Task<List<BackupFileViewModel>> GetBackupHistoryAsync()
    {
        var backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");

        if (!Directory.Exists(backupFolder))
        {
            return new List<BackupFileViewModel>();
        }

        // Get all .bak files
        var directoryInfo = new DirectoryInfo(backupFolder);
        var files = directoryInfo.GetFiles("*.bak")
                                 .OrderByDescending(f => f.CreationTime) // Newest first
                                 .ToList();

        var backupList = files.Select(f => new BackupFileViewModel
        {
            FileName = f.Name,
            CreatedAt = f.CreationTime,
            // Simple size formatting
            Size = (f.Length / 1024f / 1024f).ToString("0.00") + " MB"
        }).ToList();

        return await Task.FromResult(backupList);
    }
    public async Task RestoreDatabaseAsync(string fileName)
    {
        var backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");
        var filePath = Path.Combine(backupFolder, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException("Backup file not found.");
        }

        // 1. Get current connection string and modify it to connect to 'master'
        var currentConnString = _context.Database.GetConnectionString();
        var builder = new SqlConnectionStringBuilder(currentConnString)
        {
            InitialCatalog = "master" // Switch to master database
        };

        var masterConnectionString = builder.ConnectionString;
        var dbName = "Finote"; // Or get this dynamically if you prefer

        // 2. Prepare the T-SQL script
        // KILL sessions, RESTORE, and RESET permissions
        var sql = $@"
        USE master;
        
        -- Kick everyone off
        ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        
        -- Restore
        RESTORE DATABASE [{dbName}] 
        FROM DISK = N'{filePath}' 
        WITH REPLACE;
        
        -- Let people back in
        ALTER DATABASE [{dbName}] SET MULTI_USER;
    ";

        // 3. Execute using a raw SQL connection (bypassing EF Context for Finote)
        using (var connection = new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(sql, connection))
            {
                // Restore can take time, increase timeout to 5 minutes or more
                command.CommandTimeout = 300;
                await command.ExecuteNonQueryAsync();
            }
        }
    }

}