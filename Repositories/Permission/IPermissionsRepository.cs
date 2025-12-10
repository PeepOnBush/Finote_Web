using Finote_Web.Models;
using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Permissions
{
    public interface IPermissionsRepository
    {
        Task<PermissionsViewModel> GetPermissionsDataAsync(string userSearchString = null);

        Task UpdateRolePermissionsAsync(PermissionViewModel model);
        Task ClearActivityLogAsync();

    }
}