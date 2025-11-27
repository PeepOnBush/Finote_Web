using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class NotificationType
    {
        [Key]
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconURL { get; set; } = string.Empty;

        public ICollection<Notification>? Notifications { get; set; }
    }
}
