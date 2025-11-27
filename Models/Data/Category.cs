using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public int TransactionTypeId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string IconURL { get; set; } = string.Empty;

        public TransactionType? TransactionType { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
