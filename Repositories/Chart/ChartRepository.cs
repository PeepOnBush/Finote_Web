using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Finote_Web.Repositories.Charts
{
    public class ChartRepository : IChartRepository
    {
        private readonly FinoteDbContext _context;

        public ChartRepository(FinoteDbContext context)
        {
            _context = context;
        }

        public async Task<ChartViewModel> GetUserRegistrationsChartAsync()
        {
            // --- THIS IS THE FIX ---
            // 1. Get the single, accurate, current total number of users.
            var totalUserCount = await _context.Users.CountAsync();
            // -----------------------

            var labels = new List<string>();
            var data = new List<int>();

            // We will build a simple linear progression for display purposes.
            // This is a more realistic placeholder than pure random numbers.
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));

                // --- THIS IS THE FIX ---
                // Create a simple ramp-up to the current total.
                // For the current month (i=0), it will show the true total.
                // For previous months, it will show a fraction of the total.
                // This simulates growth over time.
                int simulatedCount = (int)(totalUserCount * ((6.0 - i) / 6.0));
                data.Add(simulatedCount);
                // -----------------------
            }

            return new ChartViewModel
            {
                PageTitle = "User Registration Report",
                PageSubTitle = "Total user count over time (simulation)", // Updated subtitle to be clear
                Labels = labels,
                Data = data
            };
        }

        public async Task<ChartViewModel> GetTransactionCreationChartAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyData = await _context.Transactions
                .Where(t => t.CreatedAt >= sixMonthsAgo)
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var labels = new List<string>();
            var data = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));
                var monthData = monthlyData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
                data.Add(monthData?.Count ?? 0);
            }

            return new ChartViewModel
            {
                PageTitle = "Notes & Wallets Creation Report",
                PageSubTitle = "New transactions created per month (last 6 months)",
                Labels = labels,
                Data = data
            };
        }

        public async Task<ChartViewModel> GetAiUsageChartAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyData = await _context.AiLogs
                .Where(t => t.UsedTime >= sixMonthsAgo)
                .GroupBy(t => new { t.UsedTime.Year, t.UsedTime.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var labels = new List<string>();
            var data = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));
                var monthData = monthlyData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
                data.Add(monthData?.Count ?? 0);
            }

            return new ChartViewModel
            {
                PageTitle = "AI Feature Usage Report",
                PageSubTitle = "AI features used per month (last 6 months)",
                Labels = labels,
                Data = data
            };
        }
    }
}