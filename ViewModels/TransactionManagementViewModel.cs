namespace Finote_Web.ViewModels
{
    // Represents a single transaction row in the table
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string CategoryName { get; set; }
        public int TransactionTypeId { get; set; }
        public string Note { get; set; }
        public DateTime TransactionDate { get; set; }
        public string WalletName { get; set; }
        public string UserName { get; set; }
    }

    // The main model for the Transaction Management page
    public class TransactionManagementViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public int TransactionCount { get; set; }

        public TransactionManagementViewModel()
        {
            Transactions = new List<TransactionViewModel>();
        }
    }
}