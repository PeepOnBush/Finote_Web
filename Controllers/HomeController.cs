using Finote_Web.Models; // Add this using statement
using Microsoft.AspNetCore.Mvc;

namespace Finote_Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["CurrentPage"] = "Dashboard";
            return View();
        }

        public IActionResult AccountManagement()
        {
            ViewData["CurrentPage"] = "AccountManagement";

            // Create and populate the ViewModel with sample data
            var viewModel = new AccountManagementViewModel
            {
                Users = new List<UserViewModel>
                {
                    new UserViewModel { Id = 1, FullName = "Dang Thinh Dai", Email = "dai.dt@example.com", Role = "Admin", AvatarUrl = "https://i.pravatar.cc/40?u=user1" },
                    new UserViewModel { Id = 2, FullName = "Nguyen Van A", Email = "nguyen.va@example.com", Role = "Editor", AvatarUrl = "https://i.pravatar.cc/40?u=user2" },
                    new UserViewModel { Id = 3, FullName = "Tran Thi B", Email = "tran.tb@example.com", Role = "User", AvatarUrl = "https://i.pravatar.cc/40?u=user3" }
                }
            };

            return View(viewModel); // Pass the model to the view
        }

        public IActionResult ReportUsers()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "User Registration Report",
                PageSubTitle = "Biểu đồ số lượng người đăng ký theo thời gian",
                ChartIcon = "fas fa-chart-bar",
                ChartTitle = "Bar Chart for User Registrations Over Time"
            };
            return View(viewModel);
        }

        public IActionResult ReportNotesWallets()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "Notes & Wallets Creation Report",
                PageSubTitle = "Biểu đồ số lượng ví/ghi chú theo thời gian",
                ChartIcon = "fas fa-chart-line",
                ChartTitle = "Chart for Notes & Wallets Created Over Time"
            };
            return View(viewModel);
        }

        public IActionResult ReportAiUsage()
        {
            ViewData["CurrentPage"] = "Statistics";
            var viewModel = new ChartViewModel
            {
                PageTitle = "AI Feature Usage Report",
                PageSubTitle = "Biểu đồ số lượt sử dụng AI",
                ChartIcon = "fas fa-brain",
                ChartTitle = "Chart for AI Feature Usage"
            };
            return View(viewModel);
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