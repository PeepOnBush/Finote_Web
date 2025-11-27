using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public string CreatedByUserId { get; set; }
        public int WalletId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }

        public Users? CreatedByUser { get; set; }
        public Users? DeletedByUser { get; set; }
        public Wallet? Wallet { get; set; }
        public Category? Category { get; set; }
    }
}
