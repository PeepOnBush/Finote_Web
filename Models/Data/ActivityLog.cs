using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        // Navigation property to the User who performed the action
        public virtual Users User { get; set; }
    }
}