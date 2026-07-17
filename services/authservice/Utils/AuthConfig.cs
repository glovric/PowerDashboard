using Microsoft.AspNetCore.Identity;
using AuthService.Models;
using AuthService.Data;
using Shared;

namespace AuthService.Utils {

    public static class IdentityConfig
    {
        public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        {
            // Configure Identity options
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    // User
                    options.User.RequireUniqueEmail = true;

                    // Password
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequiredUniqueChars = 1;

                    // Sign-in
                    options.SignIn.RequireConfirmedEmail = true;

                    // Lockout
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            // Configure application cookie (for Identity UI)
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Admin/Login";
                options.LogoutPath = "/Admin/Logout";
                options.AccessDeniedPath = "/Admin/AccessDenied";
            });

            // Add authorization policy for Admin dashboard
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
            });

            services.AddRazorPages(options =>
            {
                // Allow anonymous access to account pages
                options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");

                // Remap Identity routes
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/Admin/Login");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "/Admin/Register");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/Admin/Logout");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "/Admin/AccessDenied");

                // Map custom admin pages
                options.Conventions.AddAreaPageRoute("Identity", "/Admin/Dashboard", "/Admin/Dashboard");
                options.Conventions.AddAreaPageRoute("Identity", "/Admin/CreateUser", "/Admin/CreateUser");

                // Protect all Admin pages with "AdminOnly" policy
                options.Conventions.AuthorizeAreaFolder("Identity", "/Admin", "AdminOnly");
            });

            return services;
        }
    }

    public static class JwtConfig
    {
        public static IServiceCollection InjectJwtSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>("Frontend", configuration.GetSection("FrontJwtSettings"));
            services.Configure<JwtSettings>("Service", configuration.GetSection("ServiceJwtSettings"));
            return services;
        }
    }

}