using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Overview
{
    public interface IOverviewRepository
    {
        Task<OverviewViewModel> GetOverviewDataAsync();
    }
}