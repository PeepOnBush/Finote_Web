namespace Finote_Web.Models
{
    public class ChartViewModel
    {
        public string PageTitle { get; set; }
        public string PageSubTitle { get; set; }
        public string ChartIcon { get; set; } // e.g., "fas fa-chart-bar"
        public string ChartTitle { get; set; }

        // In a real app, you'd have properties for chart labels and data
        // public List<string> Labels { get; set; }
        // public List<int> Data { get; set; }
    }
}