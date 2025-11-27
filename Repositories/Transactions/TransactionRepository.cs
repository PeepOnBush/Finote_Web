using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Finote_Web.Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly FinoteDbContext _context;

        public TransactionRepository(FinoteDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionManagementViewModel> GetTransactionManagementDataAsync()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Category.TransactionType)
                .Include(t => t.Wallet)
                .Include(t => t.CreatedByUser)
                .Select(t => new TransactionViewModel
                {
                    Id = t.TransactionId,
                    Amount = t.Amount,
                    CategoryName = t.Category.CategoryName,
                    TransactionType = t.Category.TransactionType.TransactionTypeName,
                    Note = t.Note,
                    TransactionDate = t.TransactionTime,
                    WalletName = t.Wallet.WalletName,
                    UserName = t.CreatedByUser.UserName ?? "N/A"
                }).ToListAsync();

            var totalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

            return new TransactionManagementViewModel
            {
                Transactions = transactions,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                TransactionCount = transactions.Count
            };
        }
    }
}