using PowerService.Data;
using PowerService.Data.Seed;
using PowerService.Services;
using PowerService.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared;

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

// Register PowerDataContext with PostgreSQL connection
builder.Services.AddDbContext<PowerDataContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__PowerMigrationsHistory")
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

    builder.Services.AddScoped<FrontDataService>();
    builder.Services.AddScoped<InferenceDataService>();

    builder.Services.AddSharedJwtAuthentication(builder.Configuration);

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(AuthPolicies.FrontendPrivate, policy =>
        {
            policy.AuthenticationSchemes.Add(AuthSchemes.Frontend);
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin", "User");
        })
        .AddPolicy(AuthPolicies.Service, policy =>
        {
            policy.AuthenticationSchemes.Add(AuthSchemes.Service);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("client_id", "fastapi-service");
            policy.RequireClaim("scope", "internal_api");
        });

    builder.Services.AddControllers();
    builder.Services.AddCustomRateLimiting();
}

else if (mode.ToLower() == "seed")
{
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
        var dbContext = scope.ServiceProvider.GetRequiredService<PowerDataContext>();
        
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
        var services = scope.ServiceProvider;

        var validator = services.GetRequiredService<IConfigValidator>();
        validator.ValidateConfigSection<DataFilesOptions>(configuration, "DataFiles");
        
        // Ensure migrations are applied before seeding
        var dbContext = services.GetRequiredService<PowerDataContext>();
        Log.Information("[Database Migrations] Ensuring database is up-to-date...");
        await dbContext.Database.MigrateAsync();
        
        // Run seeding
        var seeder = services.GetRequiredService<DbSeeder>();
        await seeder.SeedPowerDataAsync();
        
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
    
    // Configure middleware pipeline
    //app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();
    
    Log.Information("Application starting...");
    await app.RunAsync();
}