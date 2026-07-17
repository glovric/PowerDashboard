using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.ComponentModel.DataAnnotations;
using DotNetEnv;
    
namespace Shared
{
    public class JwtSettings
    {
        [Required(ErrorMessage = "JWT Key cannot be empty! Make sure you set a value in settings.")]
        public string Key { get; set; } = string.Empty;
        [Required(ErrorMessage = "JWT Issuer cannot be empty! Make sure you set a value in settings.")]
        public string Issuer { get; set; } = string.Empty;
        [Required(ErrorMessage = "JWT Audience cannot be empty! Make sure you set a value in settings.")]
        public string Audience { get; set; } = string.Empty;
        [Required(ErrorMessage = "JWT ExpirationMinutes cannot be empty! Make sure you set a value in settings.")]
        public int? ExpirationMinutes { get; set; }
        [Required(ErrorMessage = "JWT RefreshMinutes cannot be empty! Make sure you set a value in settings.")]
        public int? RefreshMinutes { get; set; }
    }

    public static class AuthSchemes
    {
        public const string Frontend = "Frontend"; // Front sends JWT through HTTP cookies
        public const string Service = "Service"; // Services send JWT through Authentication Bearer
    }

    public static class AuthPolicies
    {
        public const string FrontendPublic = "FrontendPublic";  // Basic JWT auth, no roles
        public const string FrontendPrivate = "FrontendPrivate"; // Roles must be included in JWT payload
        public const string Service = "Service"; // Special service claims must be included in JWT payload
    }

    public static class Helpers
    {

        public static AuthenticationBuilder AddSharedJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {

            // Creates JWT auth rules for 
            // - Frontend (JWT found in HTTP cookies) 
            // - other Services (JWT found in Authorization Bearer header)

            var frontendJwt = configuration.GetSection("FrontJwtSettings").Get<JwtSettings>()!;
            var serviceJwt = configuration.GetSection("ServiceJwtSettings").Get<JwtSettings>()!;

            var frontendKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(frontendJwt.Key));
            var serviceKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serviceJwt.Key));

            return services
                .AddAuthentication() // Sets default Auth and Challenge schemes to Identity
                .AddJwtBearer(AuthSchemes.Frontend, options =>
                {
                    options.MapInboundClaims = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Support cookie-based tokens for frontend
                            var token = context.Request.Cookies["jwt"];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = frontendJwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = frontendJwt.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = frontendKey,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        RoleClaimType = "role",
                    };
                })
                .AddJwtBearer(AuthSchemes.Service, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = serviceJwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = serviceJwt.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = serviceKey,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {

            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            return services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials());
            });
        }

        public static void ValidateConfigSection<T>(IConfiguration config, string sectionName) where T : new()
        {
            ArgumentNullException.ThrowIfNull(config);
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException(nameof(sectionName));

            var settings = new T();
            config.GetSection(sectionName).Bind(settings);

            // Validate data annotations
            var context = new ValidationContext(settings, null, null);
            Validator.ValidateObject(settings, context, validateAllProperties: true);

            Console.WriteLine($"[ConfigValidator] Section '{sectionName}' validated successfully.");
        }

        private static string GetSharedFolderPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            
            // Search up to 5 levels up to find the "Shared" folder
            for (int i = 0; i < 5 && directory != null; i++)
            {
                var potentialSharedPath = Path.Combine(directory.FullName, "Shared");
                if (Directory.Exists(potentialSharedPath))
                {
                    return potentialSharedPath; // Return the solution root (parent of Shared)
                }
                
                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(
                "Could not find a 'Shared' folder within 5 levels up from the application base directory.");
        }

        public static void LoadDotEnvFile() {

            var sharedDir = GetSharedFolderPath();

            if (!Directory.Exists(sharedDir))
            {
                Console.WriteLine($"[Config] Warning: Shared directory '{sharedDir}' not found.");
                return;
            }

            try
            {

                var envFilePath = Path.Combine(sharedDir, ".env");

                if (File.Exists(envFilePath))
                {
                    Env.Load(envFilePath);
                    Console.WriteLine("[Config] Loaded .env file");
                }
                else
                {
                    Console.WriteLine("[Config] .env file in Shared directory not found.");
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load .env configuration.", ex);
            }

        }
       
        public static void LoadSharedSettings(IConfigurationBuilder configBuilder)
        {
            var sharedDir = GetSharedFolderPath();

            if (!Directory.Exists(sharedDir))
            {
                Console.WriteLine($"[Config] Warning: Shared directory '{sharedDir}' not found. Relying solely on Environment Variables.");
                configBuilder.AddEnvironmentVariables();
                return;
            }

            try
            {

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

                Console.WriteLine($"[Config] Environment: {environment}");

                // ---- BASE CONFIG ----
                var baseConfig = Path.Combine(sharedDir, "sharedsettings.json");

                if (File.Exists(baseConfig))
                {
                    configBuilder.AddJsonFile(baseConfig, optional: false, reloadOnChange: true);
                    Console.WriteLine("[Config] Loaded sharedsettings.json");
                }
                else
                {
                    Console.WriteLine("[Config] sharedsettings.json does not exist in Shared folder.");
                }

                // ---- ENVIRONMENT CONFIG ----
                var envConfig = Path.Combine(sharedDir, $"sharedsettings.{environment}.json");

                if (File.Exists(envConfig))
                {
                    configBuilder.AddJsonFile(envConfig, optional: true, reloadOnChange: true);
                    Console.WriteLine($"[Config] Loaded sharedsettings.{environment}.json");
                }
                else
                {
                    Console.WriteLine($"[Config] sharedsettings.{environment}.json does not exist in Shared folder.");
                }

                // ---- ENVIRONMENT VARIABLES (FINAL OVERRIDE) ----
                configBuilder.AddEnvironmentVariables();

                Console.WriteLine("[Config] Environment variables loaded as final override layer");
            }
            catch (Exception ex) when (
                ex is FileNotFoundException ||
                ex is InvalidOperationException ||
                ex is System.Text.Json.JsonException)
            {
                throw new AggregateException("Failed to load configuration from Shared folder.", ex);
            }
        }

    }

    public interface IConfigValidator
    {
        void ValidateConfigSection<T>(IConfiguration config, string sectionName) where T : new();
    }

    public class ConfigValidator : IConfigValidator
    {

        private readonly Serilog.ILogger _logger;

        public ConfigValidator(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void ValidateConfigSection<T>(IConfiguration config, string sectionName) where T : new()
        {
            ArgumentNullException.ThrowIfNull(config);

            var settings = new T();
            config.GetSection(sectionName).Bind(settings);

            var context = new ValidationContext(settings);
            Validator.ValidateObject(settings, context, true);

            _logger.Information("[ConfigValidator] Section {sectionName} validated successfully.", sectionName);
        }
        
    }

}