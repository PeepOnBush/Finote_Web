using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }
        [Required]
        public string PermissionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
