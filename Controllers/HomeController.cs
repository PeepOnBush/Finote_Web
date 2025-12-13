using Finote_API.Services.SendEmail;
using Finote_Web.Models.Data;
using Finote_Web.Repositories.Overview;
using Finote_Web.Repositories.Permissions;
using Finote_Web.Repositories.Transactions;
using Finote_Web.Repositories.UserRepo;
using Finote_Web.Repositories.Logging;
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
using Finote_Web.Repositories.Charts;

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
        public readonly FinoteDbContext _context;
        public readonly IChartRepository _chartRepo;
        public readonly IActivityLogRepository _activityLogRepository;

        public HomeController(
            IOverviewRepository overviewRepo,
            IUserRepository userRepo,
            ITransactionRepository transactionRepo,
            IPermissionsRepository permissionsRepo,
            ISettingsRepository settingsRepo,
            IEmailSenderService emailSenderService,
            IChartRepository chartRepo,
            IActivityLogRepository activityLogRepository,
            HttpClient httpClient,
            FinoteDbContext context,
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
            _context = context;
            _chartRepo = chartRepo;
            _activityLogRepository = activityLogRepository;
        }

        #region Overview / Dashboard

        [Authorize(Policy = "CanViewOverview")]
        public async Task<IActionResult> Index()
        {
            ViewData["CurrentPage"] = "Overview";
            var viewModel = await _overviewRepo.GetOverviewDataAsync();
            // 1. Total Wallets (Count how many wallets exist in the system)
            // Please ensure 'Wallets' matches your DbSet name in ApplicationDbContext
            var totalWallets = _context.Wallets.Count();

            // 2. Total AI Usage (Count rows in AiLog table)
            var totalAiUsage = _context.AiLogs.Count();

            // Pass data to View
            ViewBag.TotalWallets = totalWallets;
            ViewBag.TotalAiUsage = totalAiUsage;
            return View(viewModel);
        }

        #endregion

        #region Account Management (Admin Only)

        [Authorize(Roles = "Admin")]
        // Add the parameter to the action
        public async Task<IActionResult> AccountManagement(string searchString)
        {
            ViewData["CurrentPage"] = "AccountManagement";
            // Pass the search string to the view data so we can keep it in the input box
            ViewData["CurrentFilter"] = searchString;

            // Pass it to the repository
            var users = await _userRepo.GetAllUsersAsync(searchString);

            var viewModel = new AccountManagementViewModel
            {
                Users = users.ToList(),
                // ... (NewUser initialization remains the same) ...
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

            // 1. Get the details of the user we are about to delete (so we can log their name)
            var userToDelete = await _userManager.FindByIdAsync(id);

            if (userToDelete != null)
            {
                // Store the name for the log message
                string deletedUserName = userToDelete.UserName;
                string deletedUserEmail = userToDelete.Email;

                // 2. Perform the delete
                await _userRepo.DeleteUserAsync(id);

                // 3. Get the ID of the CURRENT ADMIN (You)
                var adminId = _userManager.GetUserId(User);

                // 4. Log the action assigned to the ADMIN
                // This log will persist because the Admin user is NOT being deleted.
                await _activityLogRepository.LogActivityAsync(adminId, $"Deleted User: {deletedUserName} ({deletedUserEmail})");
            }

            return RedirectToAction("AccountManagement");
        }

        #endregion

        #region Transaction Management

        [Authorize(Policy = "CanAccessTransactionManagement")]
        public async Task<IActionResult> TransactionManagement(
                string searchString,
                string typeFilter = "All",
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            ViewData["CurrentPage"] = "TransactionManagement";

            // Save filter state to ViewBag to repopulate the form
            ViewBag.CurrentFilter = searchString;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var viewModel = await _transactionRepo.GetTransactionManagementDataAsync(searchString, typeFilter, startDate, endDate);
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
        public async Task<IActionResult> ReportUsers()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = await _chartRepo.GetUserRegistrationsChartAsync();
            return View("ReportChart", viewModel);
        }

        // ... (Other report actions like ReportNotesWallets and ReportAiUsage would also have the [Authorize] attribute)
        [Authorize(Policy = "CanViewStatistics")]
        public async Task<IActionResult> ReportNotesWallets()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = await _chartRepo.GetTransactionCreationChartAsync();
            return View("ReportChart", viewModel);
        }

        [Authorize(Policy = "CanViewStatistics")]
        public async Task<IActionResult> ReportAiUsage()    
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = await _chartRepo.GetAiUsageChartAsync();
            return View("ReportChart", viewModel);
        }

        #endregion

        #region Authorization & Security (Admin Only)

        [Authorize(Roles = "Admin")]
        // Add activeTab parameter, defaulting to "roles"
        public async Task<IActionResult> Permissions(string searchString, string activeTab = "roles")
        {
            ViewData["CurrentPage"] = "Permissions";
            ViewData["CurrentFilter"] = searchString;

            // Pass the active tab choice to the view
            ViewData["ActiveTab"] = activeTab;

            var viewModel = await _permissionsRepo.GetPermissionsDataAsync(searchString);
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
                LastBackupDate = await _settingsRepo.GetLastBackupDateAsync(),
                BackupHistory = await _settingsRepo.GetBackupHistoryAsync()
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
                var apiBaseUrl = "http://localhost:5134"; // Or whatever your API's port is

               
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
                var backupFilePath = await _settingsRepo.BackupDatabaseAsync();

                if (string.IsNullOrEmpty(backupFilePath) || !System.IO.File.Exists(backupFilePath))
                {
                    return StatusCode(500, new { message = "Failed to create the backup file." });
                }

                // Read bytes for download
                var fileBytes = await System.IO.File.ReadAllBytesAsync(backupFilePath);
                var fileName = Path.GetFileName(backupFilePath);

                // ===== GET FILE SIZE FOR UI =====
                var fileInfo = new FileInfo(backupFilePath);
                string fileSizeString = (fileInfo.Length / 1024f / 1024f).ToString("0.00") + " MB";
                // ================================

                // IMPORTANT: If you want the file to appear in the list after reload, 
                // DO NOT delete it here. If you delete it, the list is just a visual log.
                // System.IO.File.Delete(backupFilePath); // <--- Keep this commented out if you want persistent history

                var newTimestamp = await _settingsRepo.GetLastBackupDateAsync();

                return Ok(new
                {
                    fileContents = Convert.ToBase64String(fileBytes),
                    fileName = fileName,
                    newBackupDate = newTimestamp?.ToLocalTime().ToString("g") ?? "N/A",
                    fileSize = fileSizeString // <--- Add this to the JSON
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred during backup: {ex.Message}" });
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
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RestoreDatabase(string fileName)
        {
            try
            {
                await _settingsRepo.RestoreDatabaseAsync(fileName);

                // Return success JSON instead of redirecting
                return Ok(new { message = "Restore successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Restore failed: {ex.Message}" });
            }
        }

        #endregion
    }
}