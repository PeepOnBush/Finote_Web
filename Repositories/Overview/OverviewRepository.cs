using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
            // --- Stat Card Data (remains the same) ---
            var totalUsers = await _context.Users.CountAsync();
            var totalTransactions = await _context.Transactions.CountAsync();
            var totalRevenue = await _context.Transactions
                .Where(t => t.Category.TransactionType.TransactionTypeName == "Income")
                .SumAsync(t => t.Amount);

            // --- Real-Time Chart Data Logic ---
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var expenseTypeIds = new List<int> { 2, 3, 5 };

            // Query transactions from the last 6 months
            var recentTransactions = await _context.Transactions
                .Where(t => t.TransactionTime >= sixMonthsAgo)
                .Select(t => new {
                    t.TransactionTime.Year,
                    t.TransactionTime.Month,
                    t.Amount,
                    IsExpense = expenseTypeIds.Contains(t.Category.TransactionTypeId)
                })
                .ToListAsync();

            // Group data by month in C#
            var monthlyIncome = recentTransactions
                .Where(t => !t.IsExpense)
                .GroupBy(t => new { t.Year, t.Month })
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var monthlyExpense = recentTransactions
                .Where(t => t.IsExpense)
                .GroupBy(t => new { t.Year, t.Month })
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            // Prepare labels and data lists for the chart
            var chartLabels = new List<string>();
            var incomeData = new List<decimal>();
            var expenseData = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var key = new { date.Year, date.Month };
                chartLabels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));

                incomeData.Add(monthlyIncome.ContainsKey(key) ? monthlyIncome[key] : 0);
                expenseData.Add(monthlyExpense.ContainsKey(key) ? monthlyExpense[key] : 0);
            }

            return new OverviewViewModel
            {
                TotalUsers = totalUsers,
                TransactionsProcessed = totalTransactions,
                TotalRevenue = totalRevenue,
                NewSignupsToday = 0, // Placeholder
                ChartLabels = chartLabels,
                IncomeData = incomeData,
                ExpenseData = expenseData
            };
        }
    }
}