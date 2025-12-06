public interface ISettingsRepository
{
    Task<string> GetApiKeyAsync(string keyName);
    Task UpdateApiKeyAsync(string keyName, string newKeyValue, string updatedById); // Add user ID
    Task DeleteApiKeyAsync(string keyName, string deletedById);
    Task<string> BackupDatabaseAsync();
    Task<DateTime?> GetLastBackupDateAsync(); 
    Task UpdateLastBackupDateAsync();       
}