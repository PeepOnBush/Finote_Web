using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class TransactionType
    {
        [Key]
        public int TransactionTypeId { get; set; }
        public string TransactionTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Category>? Categories { get; set; }

    }
}
