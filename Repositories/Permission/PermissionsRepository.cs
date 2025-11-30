using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.Permissions;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Finote_Web.Repositories.Permissions
{
    public class PermissionsRepository : IPermissionsRepository
    {
        // ===== CHANGE THIS =====
        private readonly RoleManager<IdentityRole> _roleManager;
        // =======================
        private readonly UserManager<Users> _userManager;

        private readonly FinoteDbContext _context;
        // ===== AND CHANGE THIS =====
        public PermissionsRepository(RoleManager<IdentityRole> roleManager, UserManager<Users> userManager, FinoteDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
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
            var activityLogs = await _context.ActivityLogs
               .Include(log => log.User) // Join with the Users table
               .OrderByDescending(log => log.Timestamp) // Show newest first
               .Take(50) // Limit to the last 50 logs for performance
               .Select(log => new ActivityLogViewModel
               {
                   Id = log.Id,
                   UserName = log.User.UserName,
                   Action = log.Action,
                   Timestamp = log.Timestamp.ToLocalTime() // Convert from UTC to local time for display
               })
               .ToListAsync();

            var everyRoles = await _roleManager.Roles.ToListAsync();
            var allPermissions = new List<RoleClaimViewModel>
    {
        new RoleClaimViewModel { ClaimType = AppPermissions.ViewOverview },
        new RoleClaimViewModel { ClaimType = AppPermissions.ViewStatistics },
        new RoleClaimViewModel { ClaimType = AppPermissions.AccessAccountManagement },
        new RoleClaimViewModel { ClaimType = AppPermissions.AccessTransactionManagement }
    };

            var rolePermissionsList = new List<PermissionViewModel>();
            foreach (var role in allRoles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var roleClaimsViewModel = new List<RoleClaimViewModel>();
                foreach (var permission in allPermissions)
                {
                    roleClaimsViewModel.Add(new RoleClaimViewModel
                    {
                        ClaimType = permission.ClaimType,
                        // Check if the role currently has this claim
                        IsSelected = roleClaims.Any(c => c.Type == permission.ClaimType)
                    });
                }
                rolePermissionsList.Add(new PermissionViewModel { RoleName = role.Name, RoleClaims = roleClaimsViewModel });
            }
            var viewModel = new PermissionsViewModel
            {
                Roles = roleViewModels,
                UsersWithRoles = usersWithRoles, // Add the new data
                AvailableRoles = availableRoles,   // Add the dropdown options
                ActivityLogs = activityLogs,
                RolePermissions = rolePermissionsList
            };
            
            return viewModel;
        }
        public async Task UpdateRolePermissionsAsync(PermissionViewModel model)
        {
            if (model.RoleName == "Admin")
            {
                return;
            }
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null) throw new ApplicationException("Role not found.");

            var currentClaims = await _roleManager.GetClaimsAsync(role);

            // Remove all existing permission claims
            foreach (var claim in currentClaims.Where(c => c.Type.StartsWith("Permissions.")))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Add back only the ones that were selected in the form
            foreach (var claim in model.RoleClaims.Where(c => c.IsSelected))
            {
                await _roleManager.AddClaimAsync(role, new Claim(claim.ClaimType, "true"));
            }
        }
    }
}