using System.ComponentModel.DataAnnotations;
namespace Finote_Web.Models.Data
{
    public class SystemSetting
    {
        [Key] // The name of the setting, e.g., "LastBackupTimestamp"
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}