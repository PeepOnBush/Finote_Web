//using Finote_Web.Models;
//using Finote_Web.Models.Data;
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
namespace Finote_Web.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        // Inject the repository interfaces, NOT the DbContext
        private readonly IOverviewRepository _overviewRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPermissionsRepository _permissionsRepo;
        private readonly UserManager<Users> _userManager;

        public HomeController(
            IOverviewRepository overviewRepo,
            IUserRepository userRepo,
            ITransactionRepository transactionRepo,
            IPermissionsRepository permissionsRepo, // <-- Add this
            UserManager<Users> userManager)
        {
            _overviewRepo = overviewRepo;
            _userRepo = userRepo;
            _transactionRepo = transactionRepo;
            _permissionsRepo = permissionsRepo; // <-- Add this
            _userManager = userManager;

        }

        public async Task<IActionResult> Index()
        {
            ViewData["CurrentPage"] = "Overview";
            // The controller just asks for the data it needs. Clean!
            var viewModel = await _overviewRepo.GetOverviewDataAsync();
            return View(viewModel);
        }
          
        [Authorize(Roles = "Admin")]
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
                ViewData["CurrentPage"] = "AccountManagement";
                ViewData["ShowModal"] = true;

                // --- THIS IS THE FIX ---
                // You must repopulate the dropdown list before returning the view
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
                    NewUser = newUser
                };
                return View("AccountManagement", viewModel);
            }

            await _userRepo.CreateUserAsync(newUser);
            return RedirectToAction("AccountManagement");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUserForEdit(string id) // Changed to string
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

        // ===== NEW ACTION TO PROCESS THE EDIT FORM SUBMISSION =====
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // In a real scenario, you'd return the partial view with errors.
                // For simplicity, we redirect.
                return RedirectToAction("AccountManagement");
            }
            await _userRepo.UpdateUserAsync(model);
            return RedirectToAction("AccountManagement");
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
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Get the user's current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            // Remove user from all current roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            // Add user to the new role
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Permissions");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        // Change the parameter name from "rolePermissions" to "model"
        public async Task<IActionResult> UpdateRolePermissions(List<PermissionViewModel> model)
        {
            foreach (var rolePermission in model)
            {
                await _permissionsRepo.UpdateRolePermissionsAsync(rolePermission);
            }

            return RedirectToAction("Permissions");
        }

        [Authorize(Roles = "Admin")]
        // ===== NEW ACTION FOR DELETING A USER =====
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TransactionManagement()
        {
            ViewData["CurrentPage"] = "TransactionManagement";
            var viewModel = await _transactionRepo.GetTransactionManagementDataAsync();
            return View(viewModel);

        }

        //[HttpGet] // Renamed for clarity
        //public IActionResult Statistics()
        //{
        //    // This could be a landing page for all reports
        //    return RedirectToAction("ReportUsers");
        //}
        public IActionResult ReportUsers()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "User Registration Report",
                PageSubTitle = "Biểu đồ số lượng người đăng ký theo thời gian",
                Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                Data = new List<int> { 120, 150, 180, 210, 160, 250 } // Sample data for new users per month
            };
            return View("ReportChart", viewModel); // We'll use a shared view for all charts
        }

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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Permissions()
        {
            ViewData["CurrentPage"] = "Permissions";

            // The controller is now incredibly simple. It just asks the repository for the data.
            var viewModel = await _permissionsRepo.GetPermissionsDataAsync();

            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Settings()
        {
            ViewData["CurrentPage"] = "Settings";
            var viewModel = new SettingsViewModel
            {
                SmtpHost = "smtp.example.com",
                ApiKey = "**************",
                DailyApiQuota = 1000,
                LastBackupDate = DateTime.Now.AddDays(-1)
            };
            return View(viewModel);
        }

    }
}