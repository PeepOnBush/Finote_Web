using Finote_API.Services.SendEmail;
using Finote_Web.Models.Data;
using Finote_Web.Repositories.Overview;
using Finote_Web.Repositories.Permissions;
using Finote_Web.Repositories.Transactions;
using Finote_Web.Repositories.UserRepo;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Headers; // For Authorization header
using System.Security.Claims;
using System.IO;

namespace Finote_Web.Controllers
{
    [Authorize] // Every action in this controller requires the user to be logged in.
    public class HomeController : Controller
    {
        private readonly IOverviewRepository _overviewRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPermissionsRepository _permissionsRepo;
        private readonly UserManager<Users> _userManager;
        private readonly ISettingsRepository _settingsRepo;
        private readonly IEmailSenderService _emailSenderService;
        private readonly HttpClient _httpClient;

        public HomeController(
            IOverviewRepository overviewRepo,
            IUserRepository userRepo,
            ITransactionRepository transactionRepo,
            IPermissionsRepository permissionsRepo,
            ISettingsRepository settingsRepo,
            IEmailSenderService emailSenderService,
            HttpClient httpClient,
            UserManager<Users> userManager)
        {
            _overviewRepo = overviewRepo;
            _userRepo = userRepo;
            _transactionRepo = transactionRepo;
            _permissionsRepo = permissionsRepo;
            _settingsRepo = settingsRepo;
            _emailSenderService = emailSenderService;
            _userManager = userManager;
            _httpClient = httpClient;
        }

        #region Overview / Dashboard

        [Authorize(Policy = "CanViewOverview")]
        public async Task<IActionResult> Index()
        {
            ViewData["CurrentPage"] = "Overview";
            var viewModel = await _overviewRepo.GetOverviewDataAsync();
            return View(viewModel);
        }

        #endregion

        #region Account Management (Admin Only)

        [Authorize(Policy = "CanAccessAccountManagement")]
        public async Task<IActionResult> AccountManagement()
        {
            ViewData["CurrentPage"] = "AccountManagement";
            var users = await _userRepo.GetAllUsersAsync();
            var viewModel = new AccountManagementViewModel
            {
                Users = users.ToList(),
                NewUser = new AddUserViewModel
                {
                    AvailableRoles = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Admin", Text = "Admin" },
                        new SelectListItem { Value = "Editor", Text = "Editor" },
                        new SelectListItem { Value = "User", Text = "User" }
                    }
                }
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(AddUserViewModel newUser)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, we must rebuild the entire page state
                ViewData["CurrentPage"] = "AccountManagement";
                ViewData["ShowModal"] = true;

                // Repopulate the dropdown list
                newUser.AvailableRoles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Editor", Text = "Editor" },
                    new SelectListItem { Value = "User", Text = "User" }
                };

                var users = await _userRepo.GetAllUsersAsync();
                var viewModel = new AccountManagementViewModel
                {
                    Users = users.ToList(),
                    NewUser = newUser // Pass back the invalid model to show validation errors
                };
                return View("AccountManagement", viewModel);
            }

            await _userRepo.CreateUserAsync(newUser);
            return RedirectToAction("AccountManagement");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUserForEdit(string id)
        {
            var userToEdit = await _userRepo.GetUserForEditAsync(id);
            if (userToEdit == null) return NotFound();
            return PartialView("_EditUserPartial", userToEdit);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var userDetails = await _userRepo.GetUserDetailsAsync(id);
            if (userDetails == null) return NotFound();
            return PartialView("~/Views/Home/Partials/_UserDetailsPartial.cshtml", userDetails);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AccountManagement");
            }
            await _userRepo.UpdateUserAsync(model);
            return RedirectToAction("AccountManagement");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            await _userRepo.DeleteUserAsync(id);
            return RedirectToAction("AccountManagement");
        }

        #endregion

        #region Transaction Management

        [Authorize(Policy = "CanAccessTransactionManagement")]
        public async Task<IActionResult> TransactionManagement()
        {
            ViewData["CurrentPage"] = "TransactionManagement";
            var viewModel = await _transactionRepo.GetTransactionManagementDataAsync();
            return View(viewModel);
        }

        #endregion

        #region Statistics & Reports

        //[Authorize(Policy = "CanViewStatistics")]
        //public IActionResult Statistics()
        //{
        //    return RedirectToAction("ReportUsers");
        //}

        [Authorize(Policy = "CanViewStatistics")]
        public IActionResult ReportUsers()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "User Registration Report",
                PageSubTitle = "Biểu đồ số lượng người đăng ký theo thời gian",
                Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                Data = new List<int> { 120, 150, 180, 210, 160, 250 }
            };
            return View("ReportChart", viewModel);
        }

        // ... (Other report actions like ReportNotesWallets and ReportAiUsage would also have the [Authorize] attribute)
        [Authorize(Policy = "CanViewStatistics")]
        public IActionResult ReportNotesWallets()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "Notes & Wallets Creation Report",
                PageSubTitle = "Biểu đồ số lượng ví/ghi chú theo thời gian",
                Labels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6" },
                Data = new List<int> { 850, 920, 1100, 1050, 1300, 1450 } // Sample data for new items per week
            };
            return View("ReportChart", viewModel);
        }

        [Authorize(Policy = "CanViewStatistics")]
        public IActionResult ReportAiUsage()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "AI Feature Usage Report",
                PageSubTitle = "Biểu đồ số lượt sử dụng AI",
                Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                Data = new List<int> { 3500, 4100, 3900, 5200, 4800, 6000 } // Sample data for AI uses per month
            };
            return View("ReportChart", viewModel);
        }

        #endregion

        #region Authorization & Security (Admin Only)

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Permissions()
        {
            ViewData["CurrentPage"] = "Permissions";
            var viewModel = await _permissionsRepo.GetPermissionsDataAsync();
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                return BadRequest("User ID and new role are required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Permissions");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateRolePermissions(List<PermissionViewModel> model)
        {
            foreach (var rolePermission in model)
            {
                if (rolePermission.RoleName != "Admin")
                {
                    await _permissionsRepo.UpdateRolePermissionsAsync(rolePermission);
                }
            }
            return RedirectToAction("Permissions");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ClearActivityLog()
        {
            await _permissionsRepo.ClearActivityLogAsync();
            return RedirectToAction("Permissions");
        }
        #endregion

        #region Settings (Admin Only)


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Settings()
        { // Make it async
            ViewData["CurrentPage"] = "Settings";
            var viewModel = new SettingsViewModel
            {
                ApiKey = await _settingsRepo.GetApiKeyAsync("DefaultApiKey"), // Get real key
                                                                              // ... (other properties)
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateApiKey(string apiKey)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _settingsRepo.UpdateApiKeyAsync("DefaultApiKey", apiKey, userId);

            try
            {
                // Create an HttpClient to call the API

                // IMPORTANT: This assumes your API is running at this address.
                // You should store this URL in appsettings.json
                var apiBaseUrl = "http://localhost:5134"; // Or whatever your API's port is

                // We need to pass the Admin's JWT from the API to authorize this request.
                // For now, let's assume this is not implemented and focus on the cache clear.
                // A full implementation would require the admin to log into the API first.
                // For simplicity, we can temporarily disable authorization on the API endpoint for testing.
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey); // HAVE to get the header of the apikey before sending the request
                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Cache/clear-api-key", null);
                response.EnsureSuccessStatusCode(); // Throws an exception if the API call fails

                TempData["SuccessMessage"] = "API Key updated and API cache cleared successfully!";
            }
            catch (Exception ex)
            {
                // If the API isn't running or the call fails, show a warning.
                TempData["ErrorMessage"] = $"API Key was updated in the database, but failed to clear the API's cache. Changes may take up to 5 minutes to apply. Error: {ex.Message}";
            }
            // ===========================

            return RedirectToAction("Settings");
        }

        // ===== NEW ACTION FOR DELETING THE KEY =====
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteApiKey(SettingsViewModel model)
        {
            // 1. Verify the admin's password
            var user = await _userManager.GetUserAsync(User);
            var passwordCorrect = await _userManager.CheckPasswordAsync(user, model.ConfirmPassword);

            if (!passwordCorrect)
            {
                TempData["ErrorMessage"] = "Incorrect password. API Key was not deleted.";
                return RedirectToAction("Settings");
            }

            // 2. If password is correct, proceed with deletion
            var userId = user.Id;
            await _settingsRepo.DeleteApiKeyAsync("DefaultApiKey", userId);
            TempData["SuccessMessage"] = "API Key has been cleared successfully.";
            return RedirectToAction("Settings");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDatabaseBackup()
        {
            try
            {
                // Call the repository to create the backup file
                var backupFilePath = await _settingsRepo.BackupDatabaseAsync();

                if (string.IsNullOrEmpty(backupFilePath) || !System.IO.File.Exists(backupFilePath))
                {
                    TempData["ErrorMessage"] = "Failed to create the backup file.";
                    return RedirectToAction("Settings");
                }

                // Prepare the file for download
                var memory = new MemoryStream();
                using (var stream = new FileStream(backupFilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                // Clean up the file from the server after reading it into memory
                System.IO.File.Delete(backupFilePath);

                // Return the file to the user's browser
                return File(memory, "application/octet-stream", Path.GetFileName(backupFilePath));
            }
            catch (Exception ex)
            {
                // If SQL Server permissions are wrong, the exception will be caught here.
                TempData["ErrorMessage"] = $"An error occurred during backup: {ex.Message}";
                return RedirectToAction("Settings");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SendNotification(string recipient, string subject, string message)
        {
            var usersToSend = new List<Users>();
            if (recipient.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                usersToSend = await _userManager.Users.ToListAsync();
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(recipient);
                if (user != null)
                {
                    usersToSend.Add(user);
                }
            }

            if (usersToSend.Any())
            {
                foreach (var user in usersToSend)
                {
                    await _emailSenderService.SendEmailAsync(user.Email, subject, message);
                }
                TempData["SuccessMessage"] = $"Notification sent to {usersToSend.Count} user(s).";
            }
            else
            {
                TempData["ErrorMessage"] = "No users found for the specified recipient.";
            }
            return RedirectToAction("Settings");
        }
        #endregion
    }
}