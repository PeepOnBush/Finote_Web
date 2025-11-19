using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Finote_Web.ViewModels
{
    public class AddUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // For the file upload
        public IFormFile? Avatar { get; set; }

        [Required]
        public string SelectedRole { get; set; }

        // For the dropdown list of roles
        public List<SelectListItem> AvailableRoles { get; set; } = new();
    }
}