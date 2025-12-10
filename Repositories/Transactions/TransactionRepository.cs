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

        public async Task<TransactionManagementViewModel> GetTransactionManagementDataAsync(
     string searchString = null,
     string typeFilter = "All",
     DateTime? startDate = null,
     DateTime? endDate = null)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Category.TransactionType)
                .Include(t => t.Wallet)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            // 1. Apply Search (Note, Category, User, Wallet)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.Note.Contains(searchString) ||
                                         t.Category.CategoryName.Contains(searchString) ||
                                         t.CreatedByUser.UserName.Contains(searchString) ||
                                         t.Wallet.WalletName.Contains(searchString));
            }

            // 2. Apply Type Filter
            var expenseTypeIds = new List<int> { 2, 3, 5 };
            if (typeFilter == "Income")
            {
                query = query.Where(t => !expenseTypeIds.Contains(t.Category.TransactionTypeId));
            }
            else if (typeFilter == "Expense")
            {
                query = query.Where(t => expenseTypeIds.Contains(t.Category.TransactionTypeId));
            }

            // 3. Apply Date Range
            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Add 1 day to include the end date fully (up to 23:59:59)
                var end = endDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionTime <= end);
            }

            var transactions = await query.Select(t => new TransactionViewModel
            {
                Id = t.TransactionId, 
                Amount = t.Amount,
                CategoryName = t.Category.CategoryName,
                TransactionTypeId = t.Category.TransactionTypeId,
                Note = t.Note,
                TransactionDate = t.TransactionTime,
                WalletName = t.Wallet.WalletName,
                UserName = t.CreatedByUser.UserName ?? "N/A"
            }).ToListAsync();

            // Recalculate totals based on the FILTERED results
            var totalIncome = transactions.Where(t => !expenseTypeIds.Contains(t.TransactionTypeId)).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => expenseTypeIds.Contains(t.TransactionTypeId)).Sum(t => t.Amount);

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