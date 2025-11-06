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
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // The main model for the Permissions page
    public class PermissionsViewModel
    {
        public List<RoleViewModel> Roles { get; set; }
        public List<ActivityLogViewModel> ActivityLogs { get; set; }
        // Add more properties here for the other tabs as needed

        public PermissionsViewModel()
        {
            Roles = new List<RoleViewModel>();
            ActivityLogs = new List<ActivityLogViewModel>();
        }
    }
}