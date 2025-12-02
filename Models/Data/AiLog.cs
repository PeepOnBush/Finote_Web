using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finote_Web.Models.Data
{
    public class AiLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to the Users table

        public DateTime UsedTime { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public string PersonSend { get; set; } = string.Empty; // The name of the user

        // Navigation property to the User who performed the action
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}