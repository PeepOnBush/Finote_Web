using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class WalletRole
    {
        [Key]
        public int WalletRoleId { get; set; }
        [Required]
        public string WalletRoleName { get; set; }
        public string Description { get; set; }

        public ICollection<UserWalletParticipant>? UserWalletParticipants { get; set; }
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
