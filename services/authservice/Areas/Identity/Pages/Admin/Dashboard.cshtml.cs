using AuthService.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace AuthService.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersModel(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<User> Users { get; set; } = new List<User>();
        public IList<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

        [BindProperty]
        public string SelectedRole { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserEmail { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserName { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserPassword { get; set; } = string.Empty;

        [BindProperty]
        public string UserIdToEdit { get; set; } = string.Empty;

        [BindProperty]
        public string RoleToAdd { get; set; } = string.Empty;

        [BindProperty]
        public string RoleToRemove { get; set; } = string.Empty;

        public Dictionary<string, IList<string>> UserRoles { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = _userManager.Users.ToList();
            Roles = _roleManager.Roles.ToList();

            foreach (var user in Users)
            {
                UserRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
        }

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUserEmail) || string.IsNullOrWhiteSpace(NewUserName) || string.IsNullOrWhiteSpace(NewUserPassword))
            {
                ModelState.AddModelError(string.Empty, "All fields are required");
                return Page();
            }

            var user = new User { UserName = NewUserName, Email = NewUserEmail };
            var result = await _userManager.CreateAsync(user, NewUserPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostConfirmEmailAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Mark email as confirmed
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Email for {user.Email} has been confirmed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to confirm email: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEndLockoutAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Lockout for {user.Email} has been disabled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to disable lockout: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }
    }
}
