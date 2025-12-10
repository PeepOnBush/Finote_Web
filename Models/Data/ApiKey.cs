using Finote_Web.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class ApiKey
{
    [Key]
    public int Id { get; set; }

    public string KeyName { get; set; } // e.g., "SystemKey"
    public string KeyValue { get; set; }

    // ===== NEW FIELD =====
    public bool IsDeleted { get; set; } = false;
    // =====================

    public DateTime CreatedAt { get; set; }
    public string WhoCreatedId { get; set; }

    public DateTime? DeletedAt { get; set; }
    public string? WhoDeletedId { get; set; }

    [ForeignKey("WhoCreatedId")]
    public virtual Users WhoCreated { get; set; }

    [ForeignKey("WhoDeletedId")]
    public virtual Users? WhoDeleted { get; set; }
}