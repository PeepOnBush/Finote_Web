using Finote_Web.Models;
using Finote_Web.Models.Data;
using Finote_Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Finote_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
                // Find the user by their username first.
                var user = await _userManager.FindByNameAsync(model.Email);

                // ===== THIS IS THE FIX =====
                // If the user is not found by username, try finding them by their email.
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                }
                // ===========================

                // Now, proceed with the password check on the 'user' object we found (or didn't find).
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    // The user was found and the password is correct.
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return LocalRedirect(returnUrl);
                    }
                }

                // If we reach this point, either the user was not found or the password was incorrect.
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // This deletes the authentication cookie
            return RedirectToAction("Login", "Account"); // Send the user back to the login page
        }
    }
}