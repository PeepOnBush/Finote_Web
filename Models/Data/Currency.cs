using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class Currency
    {
        [Key]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; } = string.Empty;

        public ICollection<Wallet>? Wallets { get; set; }
    }
}
