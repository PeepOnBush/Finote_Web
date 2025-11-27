using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class UserWalletParticipant
    {
        [Required]
        public int WalletId { get; set; }
        public required string UserId { get; set; }
        public int WalletRoleId { get; set; }
        public DateTime JoinAt { get; set; }
        public bool AllowNotification { get; set; }

        public Wallet? Wallet { get; set; }
        public Users? User { get; set; }
        public WalletRole? WalletRole { get; set; }
    }
}
