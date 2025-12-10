using System.ComponentModel.DataAnnotations;

namespace Finote_Web.ViewModels
{
    public class BackupFileViewModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; } // Encrypted or relative path if needed
        public DateTime CreatedAt { get; set; }
        public string Size { get; set; } // e.g., "15 MB"
    }
    public class SettingsViewModel
    {
        // Tab 1: System Configuration
        public string SmtpHost { get; set; }
        public string ApiKey { get; set; }
        public int DailyApiQuota { get; set; }

        [Required(ErrorMessage = "Password is required to confirm deletion.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
        // Tab 2: Backup
        public DateTime? LastBackupDate { get; set; }

        // Tab 3: Notification
        public string? NotificationSubject { get; set; }
        public string? NotificationBody { get; set; }
        public List<BackupFileViewModel> BackupHistory { get; set; } = new();

    }
}