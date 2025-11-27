using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Finote_Web.ViewModels
{
    public class EditUserViewModel
    {
        // Hidden field to know which user we are editing
        public string Id { get; set; }

        // We show these but don't allow editing
        public string UserName { get; set; }
        public string Email { get; set; }

        // Passwords are not required when editing, only if the admin wants to change them.
        [DataType(DataType.Password)]
        [Display(Name = "New Password (optional)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? Avatar { get; set; }

        [Required]
        public string SelectedRole { get; set; }

        public List<SelectListItem> AvailableRoles { get; set; } = new();
    }
}