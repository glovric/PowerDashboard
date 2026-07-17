using System.Threading.RateLimiting;

namespace AuthService.Utils {

    public static class RateLimiting
    {
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // 1. Global Limiter (Fallback for all endpoints)
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // 2. Login Policy (Strict, per IP)
                options.AddPolicy("LoginPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(15),
                            QueueLimit = 0
                        }));

                // 3. Register Policy (Strict, per IP)
                options.AddPolicy("RegisterPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromHours(1),
                            QueueLimit = 0
                        }));

                // 4. Refresh Token Policy (Per User)
                options.AddPolicy("RefreshPolicy", httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? "unknown",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 10,
                            AutoReplenishment = true
                        }));

                // 5. Service Token Policy (Per Client/IP)
                options.AddPolicy("ServiceTokenPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(5)
                        }));

                // Global rejection handler
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    var message = new { error = "Too many requests. Please try again later." };
                    await context.HttpContext.Response.WriteAsJsonAsync(message, token);
                };
            });

            return services;
        }
    }

}