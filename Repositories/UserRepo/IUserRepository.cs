using Finote_Web.ViewModels;

namespace Finote_Web.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();
        Task<EditUserViewModel> GetUserForEditAsync(string id); // For populating the edit form
        Task CreateUserAsync(AddUserViewModel newUser);
        Task UpdateUserAsync(EditUserViewModel userToUpdate);
        Task<UserDetailsViewModel> GetUserDetailsAsync(string id);

        Task DeleteUserAsync(string id);
    }
}