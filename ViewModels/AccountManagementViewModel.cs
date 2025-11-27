using Finote_Web.ViewModels;

namespace Finote_Web.ViewModels
{
    // Represents a single user row in the table
    public class UserViewModel
    {
        // CRITICAL: Changed from int to string to match ASP.NET Core Identity
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
    }
    // The main model for the Account Management page
    public class AccountManagementViewModel
    {
        // For the list of existing users
        public List<UserViewModel> Users { get; set; } = new();

        // For the "Add New User" modal form
        public AddUserViewModel NewUser { get; set; } = new();
        // For the "Edit User" modal form
        public EditUserViewModel EditUser { get; set; } = new();

    }

}