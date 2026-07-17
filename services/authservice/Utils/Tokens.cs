using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using AuthService.Models;
using AuthService.Data;
using Shared;


namespace AuthService.Tokens {

    public class TokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthDbContext _db;
        public readonly JwtSettings frontJwtSettings;
        public readonly JwtSettings serviceJwtSettings;

        public TokenService(
            AuthDbContext db,
            UserManager<User> userManager,
            IOptionsMonitor<JwtSettings> jwtOptions)
        {
            _db = db;
            _userManager = userManager;
            frontJwtSettings = jwtOptions.Get("Frontend");
            serviceJwtSettings = jwtOptions.Get("Service");
        }

        public async Task<string> GenerateJwtAccessToken(User user)
        {

            var UserName = user.UserName ?? throw new InvalidOperationException("UserName cannot be null");
            var Email = user.Email ?? throw new InvalidOperationException("Email cannot be null");

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, UserName),
                    new Claim(JwtRegisteredClaimNames.Email, Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(frontJwtSettings.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: frontJwtSettings.Issuer,
                audience: frontJwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(frontJwtSettings.ExpirationMinutes!.Value),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            string refreshToken = GenerateSecureToken();

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(refreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(frontJwtSettings.RefreshMinutes!.Value)
            };

            _db.RefreshTokens.Add(entity);
            await _db.SaveChangesAsync();

            return refreshToken;
        }

        public string GenerateServiceToken()
        {
            var claims = new List<Claim>
            {
                new Claim("client_id", "fastapi-service"),
                new Claim("scope", "internal_api")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(serviceJwtSettings.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: serviceJwtSettings.Issuer,
                audience: serviceJwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(serviceJwtSettings.ExpirationMinutes!.Value),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool Success, User user, string NewRefreshToken)>
            ValidateAndRotateAsync(string refreshToken)
        {
            var tokenHash = Hash(refreshToken);

            var existing = await _db.RefreshTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    t.RevokedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (existing == null)
                return (false, null!, null!);

            // revoke old token
            existing.RevokedAt = DateTime.UtcNow;

            // generate replacement
            var newRefreshToken = GenerateSecureToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existing.UserId,
                TokenHash = Hash(newRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(frontJwtSettings.RefreshMinutes!.Value)
            });

            await _db.SaveChangesAsync();

            return (true, existing.User, newRefreshToken);
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var tokenHash = Hash(refreshToken);

            var token = await _db.RefreshTokens
                .SingleOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    t.RevokedAt == null);

            if (token == null)
                return;

            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }

    }

}