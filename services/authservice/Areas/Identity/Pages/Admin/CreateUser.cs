using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthService.Data.Entities;

namespace AuthService.Pages.Admin
{
    public class CreateUserModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateUserModel(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

        [BindProperty]
        public string NewUserEmail { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserName { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserPassword { get; set; } = string.Empty;

        [BindProperty]
        public string NewUserRole { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            Roles = _roleManager.Roles.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUserEmail) || 
                string.IsNullOrWhiteSpace(NewUserName) || 
                string.IsNullOrWhiteSpace(NewUserPassword))
            {
                ModelState.AddModelError(string.Empty, "All fields are required");
                return Page();
            }

            var user = new User { UserName = NewUserName, Email = NewUserEmail };
            var result = await _userManager.CreateAsync(user, NewUserPassword);
            
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(NewUserRole))
                {
                    if (!await _roleManager.RoleExistsAsync(NewUserRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(NewUserRole));
                    }

                    await _userManager.AddToRoleAsync(user, NewUserRole);
                }

                return RedirectToPage("/Admin/Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToPage("/Admin/Dashboard");
        }
    }
}