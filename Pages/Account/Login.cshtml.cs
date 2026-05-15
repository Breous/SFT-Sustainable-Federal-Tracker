using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using SFT.Models;

namespace SFT.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Clearing previous TempData to ensure a clean state
            TempData.Remove("Error");
            TempData.Remove("Success");

            var result = await _signInManager.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Hard-coded path to the Profile page we just fixed
                return RedirectToPage("/Profile");
            }

            // Handling Failure
            TempData["Error"] = "Invalid login attempt. Please check your Axiom credentials.";
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return Page();
        }
    }
}