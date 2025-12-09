using Finote_Web.Models;
using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Charts
{
    public interface IChartRepository
    {
        Task<ChartViewModel> GetUserRegistrationsChartAsync();
        Task<ChartViewModel> GetTransactionCreationChartAsync();
        Task<ChartViewModel> GetAiUsageChartAsync();
    }
}