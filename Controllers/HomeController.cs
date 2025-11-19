
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Finote_Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["CurrentPage"] = "Dashboard";

            // Create and populate the ViewModel with sample data
            var viewModel = new OverviewViewModel
            {
                // Stat card data
                TotalUsers = 1250,
                TotalRevenue = 45890,
                TransactionsProcessed = 8430,
                NewSignupsToday = 72,

                // Chart data
                ChartLabels = new List<string> { "January", "February", "March", "April", "May", "June" },
                IncomeData = new List<decimal> { 5200, 5500, 5300, 5800, 6200, 6500 },
                ExpenseData = new List<decimal> { 3100, 3400, 3200, 3800, 4000, 4100 }
            };

            return View(viewModel);
        }

        public IActionResult AccountManagement()
        {
            ViewData["CurrentPage"] = "AccountManagement";

            var viewModel = PrepareAccountManagementViewModel(); // Use a helper method


            return View(viewModel);
        }
        [HttpPost]
        public IActionResult CreateUser(AddUserViewModel newUser) // Renamed for clarity
        {
            // Check if the submitted data is valid based on the model's annotations
            if (!ModelState.IsValid)
            {
                // --- THIS IS THE FIX ---
                // If validation fails, we must rebuild the ENTIRE page's data
                var viewModel = PrepareAccountManagementViewModel();

                // Replace the empty form model with the one the user submitted, which contains the errors
                viewModel.NewUser = newUser;

                // Tell the View to show the modal immediately on page load
                ViewData["ShowModal"] = true;

                // Return the main view, not a redirect. This preserves validation errors.
                return View("AccountManagement", viewModel);
            }

            // --- SIMULATE SUCCESSFUL CREATION ---
            Console.WriteLine($"New user created: {newUser.UserName}, Role: {newUser.SelectedRole}");

            // On success, we still redirect to prevent form re-submission on refresh (Post/Redirect/Get pattern)
            return RedirectToAction("AccountManagement");
        }
        // Code tạo model ảo sài lại cho tiện AccountManagementViewModel
        private AccountManagementViewModel PrepareAccountManagementViewModel()
        {
            var existingUsers = new List<UserViewModel>
        {
            new UserViewModel { Id = 1, FullName = "Dang Thinh Dai", Email = "dai.dt@example.com", Role = "Admin", AvatarUrl = "https://i.pravatar.cc/40?u=user1" },
            new UserViewModel { Id = 2, FullName = "Nguyen Van A", Email = "nguyen.va@example.com", Role = "Editor", AvatarUrl = "https://i.pravatar.cc/40?u=user2" },
            new UserViewModel { Id = 3, FullName = "Tran Thi B", Email = "tran.tb@example.com", Role = "User", AvatarUrl = "https://i.pravatar.cc/40?u=user3" }
        };

            var newUserForm = new AddUserViewModel
            {
                AvailableRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Editor", Text = "Editor" },
                new SelectListItem { Value = "User", Text = "User" }
            }
            };

            return new AccountManagementViewModel
            {
                Users = existingUsers,
                NewUser = newUserForm
            };
        }
        // ===== NEW ACTION TO GET USER DATA FOR THE EDIT MODAL =====
        [HttpGet]
        public IActionResult GetUserForEdit(int id)
        {
            // In a real app, you would fetch this user from the database using the id.
            // For now, we'll create a sample user.
            var userToEdit = new EditUserViewModel
            {
                Id = id,
                UserName = "nguyen.va", // This would come from the database
                Email = "nguyen.va@example.com", // This would also come from the database
                FullName = "Nguyen Van A",
                PhoneNumber = "0123456789",
                SelectedRole = "Editor", // Current role
                AvailableRoles = new List<SelectListItem> // The list of all possible roles
        {
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Editor", Text = "Editor" },
            new SelectListItem { Value = "User", Text = "User" }
        }
            };

            // We return a PartialView. This is a special type of view that doesn't
            // have a layout. It's perfect for injecting into a modal's body with JavaScript.
            return PartialView("_EditUserPartial", userToEdit);
        }

        // ===== NEW ACTION TO PROCESS THE EDIT FORM SUBMISSION =====
        [HttpPost]
        public IActionResult UpdateUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails on the server, the JS should ideally prevent this,
                // but as a fallback, we redirect. A more advanced setup might return JSON errors.
                return RedirectToAction("AccountManagement");
            }

            // --- SIMULATE UPDATING THE USER ---
            Console.WriteLine($"Updating user ID: {model.Id}");
            Console.WriteLine($"New FullName: {model.FullName}");
            Console.WriteLine($"New Role: {model.SelectedRole}");
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                Console.WriteLine("Password has been changed.");
            }
            // --- END SIMULATION ---

            return RedirectToAction("AccountManagement");
        }
        public IActionResult TransactionManagement()
        {
            ViewData["CurrentPage"] = "TransactionManagement";

            // Create and populate the ViewModel with sample data
            var viewModel = new TransactionManagementViewModel
            {
                Transactions = new List<TransactionViewModel>
        {
            new TransactionViewModel { Id = 101, Amount = 50.00m, CategoryName = "Food", TransactionType = "Expense", Note = "Lunch with team", TransactionDate = DateTime.Now.AddDays(-1), WalletName = "Personal", UserName = "Dang Thinh Dai" },
            new TransactionViewModel { Id = 102, Amount = 1200.00m, CategoryName = "Salary", TransactionType = "Income", Note = "Monthly salary", TransactionDate = DateTime.Now.AddDays(-2), WalletName = "Personal", UserName = "Dang Thinh Dai" },
            new TransactionViewModel { Id = 103, Amount = 75.50m, CategoryName = "Groceries", TransactionType = "Expense", Note = "Weekly shopping", TransactionDate = DateTime.Now.AddDays(-3), WalletName = "Shared Wallet", UserName = "Nguyen Van A" },
            new TransactionViewModel { Id = 104, Amount = 25.00m, CategoryName = "Transport", TransactionType = "Expense", Note = "Bus fare", TransactionDate = DateTime.Now.AddDays(-4), WalletName = "Personal", UserName = "Tran Thi B" }
        },
                TotalIncome = 1200.00m,
                TotalExpense = 150.50m,
                TransactionCount = 4
            };

            return View(viewModel);
        }
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

        public IActionResult Permissions()
        {
            ViewData["CurrentPage"] = "Permissions";
            var viewModel = new PermissionsViewModel
            {
                Roles = new List<RoleViewModel>
                {
                    new RoleViewModel { Name = "Admin", UserCount = 5 },
                    new RoleViewModel { Name = "Editor", UserCount = 12 },
                    new RoleViewModel { Name = "User", UserCount = 1233 }
                },
                ActivityLogs = new List<ActivityLogViewModel>
                {
                    new ActivityLogViewModel { UserEmail = "dai.dt@example.com", Action = "Logged in", Timestamp = DateTime.Now.AddMinutes(-5) },
                    new ActivityLogViewModel { UserEmail = "nguyen.va@example.com", Action = "Updated user 'Tran Thi B'", Timestamp = DateTime.Now.AddMinutes(-10) }
                }
            };
            return View(viewModel);
        }

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