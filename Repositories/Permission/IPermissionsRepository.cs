using Finote_Web.Models;
using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.Permissions
{
    public interface IPermissionsRepository
    {
        Task<PermissionsViewModel> GetPermissionsDataAsync();
        Task UpdateRolePermissionsAsync(PermissionViewModel model);

    }
}