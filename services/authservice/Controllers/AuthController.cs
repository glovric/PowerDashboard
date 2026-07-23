using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;
using AuthService.Data.Entities;
using AuthService.Requests;
using AuthService.Services;
using Shared;

namespace AuthService.Controllers {

    [ApiController]
    [Route("auth")]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IdentityOptions _identityOptions;
        private readonly TokenService _tokenService;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<IdentityOptions> identityOptionsAccessor,
            TokenService tokenService) : base(logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _identityOptions = identityOptionsAccessor.Value;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")] // Overrides global for login
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (!user.EmailConfirmed)
                return Unauthorized("Your email address must be confirmed by admin.");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                lockoutOnFailure: true
            );

            if (result.IsLockedOut)
            {
                var now = DateTimeOffset.UtcNow;
                var lockoutEnd = user.LockoutEnd ?? now;
                var minutesLeft = Math.Ceiling((lockoutEnd - now).TotalMinutes);
                return Unauthorized($"Account locked. Try again in {minutesLeft} minute(s).");
            }

            if (!result.Succeeded && user.LockoutEnabled)
            {
                // Not locked yet — show attempts remaining
                var maxAttempts = _identityOptions.Lockout.MaxFailedAccessAttempts;
                var attemptsLeft = maxAttempts - user.AccessFailedCount;

                // Ensure it's not negative (edge case)
                attemptsLeft = Math.Max(0, attemptsLeft);
                return Unauthorized($"Invalid username or password. {attemptsLeft} attempt(s) remaining.");
            }

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password.");
            }

            var access = await _tokenService.GenerateJwtAccessToken(user);
            var refresh = await _tokenService.GenerateRefreshToken(user);

            Response.Cookies.Append("jwt", access, 
                CreateAuthCookieOptions(TimeSpan.FromMinutes(_tokenService.frontJwtSettings.ExpirationMinutes!.Value))
            );
            Response.Cookies.Append("refresh", refresh, 
                CreateAuthCookieOptions(TimeSpan.FromMinutes(_tokenService.frontJwtSettings.RefreshMinutes!.Value))
            );

            return Ok(new { message = "Logged in successfully" });
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {

            if (!Request.Cookies.TryGetValue("refresh", out string? refreshToken))
            {
                // No refresh token? Still clear cookies to be safe.
                ClearAuthCookies();
                return Ok();
            }

            // 2. Revoke the refresh token in your database
            await _tokenService.RevokeRefreshToken(refreshToken);

            // 3. Clear cookies
            ClearAuthCookies();

            return Ok();
        }

        [HttpGet("refresh")]
        [EnableRateLimiting("RefreshPolicy")] // Overrides global for refresh
        public async Task<IActionResult> Refresh()
        {

            if (!Request.Cookies.TryGetValue("refresh", out var refreshToken)) {
                return Unauthorized();
            }

            var (success, user, newRefreshToken) = await _tokenService.ValidateAndRotateAsync(refreshToken);

            if(!success) {
                return Unauthorized();
            }

            var newAccessToken = await _tokenService.GenerateJwtAccessToken(user);

            Response.Cookies.Append("jwt", newAccessToken, 
                CreateAuthCookieOptions(TimeSpan.FromMinutes(_tokenService.frontJwtSettings.ExpirationMinutes!.Value))
            );
            Response.Cookies.Append("refresh", newRefreshToken, 
                CreateAuthCookieOptions(TimeSpan.FromMinutes(_tokenService.frontJwtSettings.RefreshMinutes!.Value))
            );

            return Ok();
            
        }

        [HttpPost("register")]
        [EnableRateLimiting("RegisterPolicy")] // Overrides global for register
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Username, email, and password are required.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                return Conflict("Email already registered.");

            var existingUserByName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByName != null)
                return Conflict("Username already taken.");

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Registration failed.", errors });
            }
            return Ok("User registered successfully.");
        }

        [HttpGet("getuser")]
        [Authorize(Policy = AuthPolicies.FrontendPublic)]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userName =  User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var roles = User.FindAll("role").Select(c => c.Value).ToArray();

            return Ok(new
            {
                username = userName,
                email = email,
                roles = roles
            });
        }

        [HttpPost("getservicetoken")]
        [EnableRateLimiting("ServiceTokenPolicy")]
        public IActionResult GetServiceToken()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                return Unauthorized();
            }

            var providedKey = apiKeyHeader.ToString();
            var originalKey = _configuration.GetSection("ServiceApiKeys").GetValue<string>("InferenceService")!;

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(providedKey),
                    Encoding.UTF8.GetBytes(originalKey)))
            {
                return Unauthorized();
            }

            var token = _tokenService.GenerateServiceToken();
            return Ok(new { access_token = token });
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetServiceHealth()
        {
            return NoContent();
        }

        private void ClearAuthCookies()
        {
            CookieOptions options = new()
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };
            Response.Cookies.Delete("jwt", options);
            Response.Cookies.Delete("refresh", options);
        }

        private static CookieOptions CreateAuthCookieOptions(TimeSpan? maxAge = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = maxAge
            };
        }

    }

}