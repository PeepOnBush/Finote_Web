using Microsoft.AspNetCore.Mvc.Rendering;
namespace Finote_Web.ViewModels
{
    // Represents a single role in the role list tab
    public class RoleViewModel
    {
        public string Name { get; set; }
        public int UserCount { get; set; }
    }

    // Represents an entry in the activity log tab
    public class ActivityLogViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CurrentRole { get; set; }
    }

    // The main model for the Permissions page
    public class PermissionsViewModel
    {
        public List<RoleViewModel> Roles { get; set; }
        public List<UserRoleViewModel> UsersWithRoles { get; set; } = new();
        public List<SelectListItem> AvailableRoles { get; set; } = new();

        public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();
        // Add more properties here for the other tabs as needed



        public PermissionsViewModel()
        {
            Roles = new List<RoleViewModel>();
            ActivityLogs = new List<ActivityLogViewModel>();
        }
    }
   
}