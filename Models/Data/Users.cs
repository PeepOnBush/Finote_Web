using Microsoft.AspNetCore.Identity;

namespace Finote_Web.Models.Data
{
    public class Users : IdentityUser 
    {

        public UserInfomation? UserInfomation { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Transaction>? DeletedTransactions { get; set; }
        public ICollection<Wallet>? Wallets { get; set; }
        public ICollection<UserWalletParticipant>? UserWalletParticipants { get; set; }
        public ICollection<UserNotification>? UserNotifications { get; set; }
    }
}
