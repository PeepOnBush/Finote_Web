using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<TransactionManagementViewModel> GetTransactionManagementDataAsync();
    }
}