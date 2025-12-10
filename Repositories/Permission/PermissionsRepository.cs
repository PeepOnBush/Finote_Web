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

        public async Task<PermissionsViewModel> GetPermissionsDataAsync(string userSearchString = null)
        {
            // =========================================================
            // 1. ROLE LIST TAB (Count users per role)
            // =========================================================
            var roleViewModels = new List<RoleViewModel>();

            // We access the built-in Identity roles via the RoleManager
            var allRoles = await _roleManager.Roles.ToListAsync();

            foreach (var role in allRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleViewModels.Add(new RoleViewModel { Name = role.Name, UserCount = usersInRole.Count });
            }

            // =========================================================
            // 2. ASSIGN ROLE TAB (Searchable User List)
            // =========================================================
            var usersWithRoles = new List<UserRoleViewModel>();

            // Start query and Include UserInfomation so we can search/display the Full Name
            var userQuery = _userManager.Users
                .Include(u => u.UserInfomation)
                .AsQueryable();

            // Apply Search Logic (UserName OR Email OR FullName)
            if (!string.IsNullOrEmpty(userSearchString))
            {
                userQuery = userQuery.Where(u =>
                    u.UserName.Contains(userSearchString) ||
                    u.Email.Contains(userSearchString) ||
                    (u.UserInfomation != null && u.UserInfomation.FullName.Contains(userSearchString))
                );
            }

            var allUsers = await userQuery.ToListAsync();

            foreach (var user in allUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id.ToString(), // Handles int or string ID safely
                                                 // Display FullName if available, otherwise Username
                    UserName = user.UserInfomation?.FullName ?? user.UserName,
                    CurrentRole = userRoles.FirstOrDefault() ?? "No Role"
                });
            }

            // Dropdown list for the view
            var availableRoles = allRoles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

            // =========================================================
            // 3. ACTIVITY LOG TAB
            // =========================================================
            var activityLogs = await _context.ActivityLogs
               .Include(log => log.User)
               .OrderByDescending(log => log.Timestamp)
               .Take(50)
               .Select(log => new ActivityLogViewModel
               {
                   Id = log.Id,
                   UserName = log.User.UserName,
                   Action = log.Action,
                   Timestamp = log.Timestamp.ToLocalTime()
               })
               .ToListAsync();

            // =========================================================
            // 4. EDIT PERMISSIONS TAB
            // =========================================================

            // Define the list of permissions we want to manage (using your Authorization constants)
            var allPermissionsToCheck = new List<string>
    {
        AppPermissions.ViewOverview,
        AppPermissions.ViewStatistics,
        AppPermissions.AccessAccountManagement,
        AppPermissions.AccessTransactionManagement
    };

            var rolePermissionsList = new List<PermissionViewModel>();

            // Loop through every existing Identity Role
            foreach (var role in allRoles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var roleClaimsViewModel = new List<RoleClaimViewModel>();

                // For this role, check if they have each specific permission
                foreach (var permission in allPermissionsToCheck)
                {
                    roleClaimsViewModel.Add(new RoleClaimViewModel
                    {
                        ClaimType = permission,
                        // Check if the role currently has this claim in the database
                        IsSelected = roleClaims.Any(c => c.Type == permission)
                    });
                }
                rolePermissionsList.Add(new PermissionViewModel { RoleName = role.Name, RoleClaims = roleClaimsViewModel });
            }

            // =========================================================
            // 5. ASSEMBLE FINAL VIEW MODEL
            // =========================================================
            var viewModel = new PermissionsViewModel
            {
                Roles = roleViewModels,
                UsersWithRoles = usersWithRoles,
                AvailableRoles = availableRoles,
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
        public async Task ClearActivityLogAsync()
        {
            // A very efficient way to delete all rows from a table
            await _context.ActivityLogs.ExecuteDeleteAsync();
        }
    }
}