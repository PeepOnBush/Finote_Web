using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.Repositories.Logging;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finote_Web.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly FinoteDbContext _context;
        private readonly UserManager<Users> _userManager; // <-- Inject UserManager
        private readonly IActivityLogRepository _logRepository;
        public UserRepository(FinoteDbContext context, UserManager<Users> userManager, IActivityLogRepository logRepository)
        {
            _context = context;
            _userManager = userManager;
            _logRepository = logRepository; 
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.Include(u => u.UserInfomation).ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.UserInfomation?.FullName ?? user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A", // Get the first role
                    AvatarUrl = user.UserInfomation?.AvatarUrl ?? "https://i.pravatar.cc/40"
                });
            }
            return userViewModels;
        }
        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync(string searchString = null)
        {
            // Start with the base query
            var query = _userManager.Users.Include(u => u.UserInfomation).AsQueryable();

            // Apply Search Filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // Search by Username, Email, or FullName
                query = query.Where(u => u.UserName.Contains(searchString)
                                      || u.Email.Contains(searchString)
                                      || (u.UserInfomation != null && u.UserInfomation.FullName.Contains(searchString)));
            }

            var users = await query.ToListAsync();

            // ... (Mapping logic remains the same) ...
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id, // Now an int
                    FullName = user.UserInfomation?.FullName ?? user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A",
                    AvatarUrl = user.UserInfomation?.AvatarUrl ?? "https://i.pravatar.cc/40"
                });
            }
            return userViewModels;
        }

        public async Task<UserDetailsViewModel> GetUserDetailsAsync(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.UserInfomation)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.UserInfomation?.FullName ?? "N/A",
                Gender = user.UserInfomation?.Gender ?? "N/A",
                DateOfBirth = user.UserInfomation?.DateOfBirth.ToString() ?? "N/A",
                Role = roles.FirstOrDefault() ?? "No Role",
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd
            };
        }
        public async Task CreateUserAsync(AddUserViewModel newUser)
        {
            var user = new Users
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                EmailConfirmed = true,

            };
            // UserManager handles password hashing automatically
            var result = await _userManager.CreateAsync(user, newUser.Password);

            if (result.Succeeded)
            {
                // Assign the selected role to the new user
                await _userManager.AddToRoleAsync(user, newUser.SelectedRole); 
                await _logRepository.LogActivityAsync(user.Id, $"Account '{user.UserName}' '({user.Email})' Created");
            }  
            else
            {
                // If creation fails, throw an exception to be caught by the controller
                throw new ApplicationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            var userTemp = await _userManager.FindByEmailAsync(newUser.Email);
            var userinfo = new UserInfomation
            {
                UserInfomationId = userTemp!.Id,
                FullName = newUser.FullName,
                Gender = newUser.Gender,
                DateOfBirth = newUser.DateOfBirth ?? DateTime.Now,
            };
            await  _context.UserInfomations.AddAsync(userinfo);
            await _context.SaveChangesAsync();
        }

        public async Task<EditUserViewModel> GetUserForEditAsync(string id)
        {
            var user = await _userManager.Users.Include(u => u.UserInfomation)
                                         .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.UserInfomation?.FullName,
                PhoneNumber = user.PhoneNumber,
                SelectedRole = roles.FirstOrDefault(),
                AvailableRoles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Editor", Text = "Editor" },
                    new SelectListItem { Value = "User", Text = "User" }
                }
            };
        }

        public async Task UpdateUserAsync(EditUserViewModel userToUpdate)
        {
            var user = await _context.Users
                              .Include(u => u.UserInfomation) // Eagerly load the UserInfomation
                              .FirstOrDefaultAsync(u => u.Id == userToUpdate.Id);
            // ===========================

            if (user == null) throw new ApplicationException("User not found.");

            // This is a safety check. If a User exists but somehow has no UserInfomation record,
            // we should create one before trying to update it.
            if (user.UserInfomation == null)
            {
                user.UserInfomation = new UserInfomation { UserInfomationId = user.Id };
                _context.UserInfomations.Add(user.UserInfomation);
            }

            // Now this line is safe because we've guaranteed user.UserInfomation is not null.
            user.UserInfomation.FullName = userToUpdate.FullName;
            user.PhoneNumber = userToUpdate.PhoneNumber;

            // The rest of your method can remain the same
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new ApplicationException("Failed to update user info.");

            // Update password if a new one was provided
            if (!string.IsNullOrWhiteSpace(userToUpdate.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, userToUpdate.NewPassword);
                if (!passwordResult.Succeeded) throw new ApplicationException("Failed to update password.");
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, userToUpdate.SelectedRole);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return;

            // ===== 1. CHECK FOR WALLETS =====
            var hasWallets = await _context.Wallets.AnyAsync(w => w.UserId.Equals(id));
            if (hasWallets)
            {
                throw new InvalidOperationException($"Cannot delete user '{user.UserName}'. They still have active Wallets.");
            }

            // ===== 2. CHECK FOR TRANSACTIONS =====
            // Check if they created transactions (even if wallet is gone)
            var hasTransactions = await _context.Transactions.AnyAsync(t => t.CreatedByUserId.Equals(id));
            if (hasTransactions)
            {
                throw new InvalidOperationException($"Cannot delete user '{user.UserName}'. They have recorded Transactions history.");
            }

            // ===== 3. PROCEED IF SAFE =====
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) throw new ApplicationException("Failed to delete user.");
        }
    }
}