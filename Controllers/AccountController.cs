using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Finote_Web.Controllers
{
    public class AccountController : Controller
    {
        // This action shows the login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // This action handles the form submission
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // For now, we will not validate. 
            // We'll just redirect to the main dashboard page to simulate a successful login.
            return RedirectToAction("Index", "Home");
        }

        // This action shows the forgot password page
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}