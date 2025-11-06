namespace Finote_Web.ViewModels
{
    // Represents a single user row in the table
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
    }

    // The main model for the Account Management page
    public class AccountManagementViewModel
    {
        public List<UserViewModel> Users { get; set; }

        public AccountManagementViewModel()
        {
            Users = new List<UserViewModel>();
        }
    }
}