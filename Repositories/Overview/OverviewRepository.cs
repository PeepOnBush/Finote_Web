using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Finote_Web.Repositories.Overview
{
    public class OverviewRepository : IOverviewRepository
    {
        private readonly FinoteDbContext _context;

        public OverviewRepository(FinoteDbContext context)
        {
            _context = context;
        }

        public async Task<OverviewViewModel> GetOverviewDataAsync()
        {
            // Querying real data from the database
            var totalUsers = await _context.Users.CountAsync();
            var totalTransactions = await _context.Transactions.CountAsync();

            var income = await _context.Transactions
                .Where(t => t.Category.TransactionType.TransactionTypeName == "Income")
                .SumAsync(t => t.Amount);

            var expense = await _context.Transactions
                .Where(t => t.Category.TransactionType.TransactionTypeName == "Expense")
                .SumAsync(t => t.Amount);

            // Fake data for chart until we have more time-series data
            var labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            var incomeData = new List<decimal> { 5200, 5500, 5300, 5800, 6200, 6500 };
            var expenseData = new List<decimal> { 3100, 3400, 3200, 3800, 4000, 4100 };

            return new OverviewViewModel
            {
                TotalUsers = totalUsers,
                TransactionsProcessed = totalTransactions,
                TotalRevenue = income, // Using real aggregate
                NewSignupsToday = 0, // Placeholder
                ChartLabels = labels,
                IncomeData = incomeData,
                ExpenseData = expenseData
            };
        }
    }
}