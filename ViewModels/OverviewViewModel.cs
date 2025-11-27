namespace Finote_Web.ViewModels
{

    public class OverviewViewModel
    {
        // Data for the top stat cards
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TransactionsProcessed { get; set; }
        public int NewSignupsToday { get; set; }

        // Data for the financial performance chart
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> IncomeData { get; set; } = new();
        public List<decimal> ExpenseData { get; set; } = new();
        public List<decimal> ProfitData { get; set; } = new();
    }
}
