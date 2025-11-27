using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Finote_Web.Models.Data
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int TypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public NotificationType? NotificationType { get; set; }
        public ICollection<UserNotification>? UserNotifications { get; set; }
    }
}
