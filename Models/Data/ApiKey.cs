using Finote_Web.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class ApiKey
{
    [Key]
    public int Id { get; set; }
    public string KeyName { get; set; } // e.g., "DefaultApiKey"
    public string KeyValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public string WhoCreatedId { get; set; } // Foreign key to the User who created it

    public DateTime? DeletedAt { get; set; } // Nullable, only set when "deleted"
    public string? WhoDeletedId { get; set; } // Nullable foreign key

    // Navigation properties
    [ForeignKey("WhoCreatedId")]
    public virtual Users WhoCreated { get; set; }

    [ForeignKey("WhoDeletedId")]
    public virtual Users? WhoDeleted { get; set; }
}