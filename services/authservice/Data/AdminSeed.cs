using Microsoft.AspNetCore.Identity;
using AuthService.Models;

namespace AuthService.Data
{
    public class DbSeeder(
        ILogger<DbSeeder> logger,
        IConfiguration configuration,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        public async Task SeedAdmin()
        {

            // Create User role
            if (!await roleManager.RoleExistsAsync("User")) {
                logger.LogInformation("[Database Seeding] Creating User role.");
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Create Admin role
            if (!await roleManager.RoleExistsAsync("Admin")) {
                logger.LogInformation("[Database Seeding] Creating Admin role.");
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Load SU settings from .env or JSON
            SuperUserSettings suSettings = configuration.GetSection("SuperUserSettings").Get<SuperUserSettings>()!;

            var adminUser = await userManager.FindByEmailAsync(suSettings.Email);

            if (adminUser == null)
            {

                logger.LogInformation("[Database Seeding] Creating Admin user.");

                adminUser = new User
                {
                    UserName = suSettings.UserName,
                    Email = suSettings.Email,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(adminUser, suSettings.Password);
                if (!result.Succeeded) {
                    logger.LogError("[Database Seeding] Failed to create Admin user.");
                    throw new Exception("Failed to create admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // Disable Admin Lockout
                adminUser.LockoutEnabled = false;
                await userManager.UpdateAsync(adminUser);

                await userManager.AddToRoleAsync(adminUser, "Admin");

                logger.LogInformation("[Database Seeding] Admin user created succesfully.");
            }
            else
            {
                logger.LogInformation("[Database Seeding] Admin user already exists.");
            }
        }

    }
    
}
