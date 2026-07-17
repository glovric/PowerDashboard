using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Utils;
using AuthService.Tokens;
using Shared;
using AuthService.Models;
using Serilog;

// Dev mode: load .env file in Shared folder if exists
Helpers.LoadDotEnvFile();

var builder = WebApplication.CreateBuilder(args);

// 1 ) Load sharedsettings.json
// 2 ) Load sharedsettings.env.json
// 3 ) Load env vars
Helpers.LoadSharedSettings(builder.Configuration);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Database connection and settings
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__AuthMigrationsHistory")
    )
);

builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddSingleton<IConfigValidator, ConfigValidator>();

var mode = Environment.GetEnvironmentVariable("MODE") ?? 
           (args.Contains("--migrate-only") ? "migrate" : 
            args.Contains("--seed-only") ? "seed" : 
            "app");

if (mode.ToLower() == "app")
{

    builder.Services.AddScoped<TokenService>();

    builder.Services.AddCustomIdentity();

    // Custom JWT
    builder.Services.InjectJwtSettings(builder.Configuration);
    builder.Services.AddSharedJwtAuthentication(builder.Configuration);

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(AuthPolicies.FrontendPublic, policy =>
        {
            policy.AuthenticationSchemes.Add(AuthSchemes.Frontend);
            policy.RequireAuthenticatedUser();
        });

    builder.Services.AddControllers();

    builder.Services.AddCustomRateLimiting();

    //builder.Services.AddFrontendCors(builder.Configuration);
}

else if (mode.ToLower() == "seed")
{
    builder.Services.AddCustomIdentity();
    builder.Services.AddScoped<DbSeeder>();
}


var app = builder.Build();

Log.Information("Starting in {Mode} mode", mode);

switch (mode.ToLower())
{
    case "migrate":
        await RunMigrationAsync(app);
        return;
        
    case "seed":
        await RunSeedingAsync(app, builder.Configuration);
        return;
        
    case "app":
    default:
        await RunApplicationAsync(app, builder.Configuration);
        break;
}

// ==================== Mode Handlers ====================

async Task RunMigrationAsync(WebApplication app)
{
    Log.Information("[Database Migrations] Started");
    
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
        Log.Information("[Database Migrations] Applying pending migrations...");
        await dbContext.Database.MigrateAsync();
        
        Log.Information("[Database Migrations] Completed Successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "[Database Migrations] Failed");
        throw;
    }
}

async Task RunSeedingAsync(WebApplication app, IConfiguration configuration)
{
    Log.Information("[Database Seeding] Started");
    
    try
    {
        using var scope = app.Services.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IConfigValidator>();
        validator.ValidateConfigSection<SuperUserSettings>(configuration, "SuperUserSettings");
        
        // Ensure migrations are applied before seeding
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Log.Information("[Database Migrations] Ensuring database is up-to-date...");
        await dbContext.Database.MigrateAsync();
        
        // Run seeding
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAdmin();
        
        Log.Information("[Database Seeding] Completed Successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "[Database Seeding] Failed");
        throw;
    }
}

async Task RunApplicationAsync(WebApplication app, IConfiguration configuration)
{
    // Validate configuration before starting
    var validator = app.Services.GetRequiredService<IConfigValidator>();
    validator.ValidateConfigSection<JwtSettings>(configuration, "FrontJwtSettings");
    validator.ValidateConfigSection<JwtSettings>(configuration, "ServiceJwtSettings");

    app.MapGet("/", context =>
    {
        context.Response.Redirect("/Admin/Dashboard");
        return Task.CompletedTask;
    });
    
    // Configure middleware pipeline
    //app.UseCors("AllowFrontend");
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();
    app.MapControllers();
    
    Log.Information("Application starting...");
    await app.RunAsync();
}