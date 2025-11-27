using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finote_Web.Models.Data
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        public required string UserId { get; set; }
        public required int CurrencyId { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Users? User { get; set; }
        public Currency? Currency { get; set; }
        public ICollection<UserWalletParticipant>? UserWalletParticipants { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}

