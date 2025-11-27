using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class RolePermission
    {
        [Required]
        public int WalletRoleId { get; set; }
        [Required]
        public int PermissionId { get; set; }

        public WalletRole? WalletRole { get; set; }
        public Permission? Permission { get; set; }
    }
}
