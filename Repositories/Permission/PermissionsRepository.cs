using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finote_Web.Repositories.Permissions
{
    public class PermissionsRepository : IPermissionsRepository
    {
        // ===== CHANGE THIS =====
        private readonly RoleManager<IdentityRole> _roleManager;
        // =======================
        private readonly UserManager<Users> _userManager;

        // ===== AND CHANGE THIS =====
        public PermissionsRepository(RoleManager<IdentityRole> roleManager, UserManager<Users> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<PermissionsViewModel> GetPermissionsDataAsync()
        {
            // --- Role Counts (existing logic is fine) ---
            var roleViewModels = new List<RoleViewModel>();
            var allRoles = await _roleManager.Roles.ToListAsync();
            foreach (var role in allRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleViewModels.Add(new RoleViewModel { Name = role.Name, UserCount = usersInRole.Count });
            }

            // --- NEW: User Role Assignments ---
            var usersWithRoles = new List<UserRoleViewModel>();
            var allUsers = await _userManager.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    CurrentRole = userRoles.FirstOrDefault() ?? "No Role"
                });
            }

            // --- NEW: List of available roles for dropdowns ---
            var availableRoles = allRoles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

            var viewModel = new PermissionsViewModel
            {
                Roles = roleViewModels,
                UsersWithRoles = usersWithRoles, // Add the new data
                AvailableRoles = availableRoles   // Add the dropdown options
            };

            return viewModel;
        }
    }
}