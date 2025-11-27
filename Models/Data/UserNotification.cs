using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class UserNotification
    {
        [Key]
        public int UserNotificationId { get; set; }
        public int NotificationId { get; set; }
        public required string UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Notification? Notification { get; set; }
        public Users? User { get; set; }
    }
}
