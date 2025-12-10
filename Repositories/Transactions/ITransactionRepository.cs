using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<TransactionManagementViewModel> GetTransactionManagementDataAsync(
                 string searchString = null,
                 string typeFilter = "All",
                 DateTime? startDate = null,
                DateTime? endDate = null);
    }
}