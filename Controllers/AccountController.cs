using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.Repositories.Logging;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Finote_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly IActivityLogRepository _logRepository;
        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, IActivityLogRepository logRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logRepository = logRepository;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // 1. Try to find user by Username OR Email in one go
                var user = await _userManager.FindByNameAsync(model.Email)
                           ?? await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // 2. Check password and sign in
                    // Note: Pass user.UserName here to be safe with all overloads
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // 3. Log Activity
                        // Make sure your IActivityLogService is injected as _logService (or _logRepository)
                        await _logRepository.LogActivityAsync(user.Id, "Logged In"); // Ensure user.Id is an int if you changed it to int!

                        return LocalRedirect(returnUrl);
                    }
                }

                // Standard failure message
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // ===== LOG THE ACTIVITY =====
            // Get the user before they are signed out
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _logRepository.LogActivityAsync(user.Id, "Logged Out");
            }
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}