namespace Finote_Web.ViewModels
{
    public class ChartViewModel
    {
        public string PageTitle { get; set; }
        public string PageSubTitle { get; set; }
        public string ChartIcon { get; set; } // e.g., "fas fa-chart-bar"
        public string ChartTitle { get; set; }

        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
    }
}