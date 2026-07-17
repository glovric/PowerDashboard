using System.Threading.RateLimiting;

namespace PowerService.Utils {

    public static class RateLimiting
    {
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Global default policy (optional)
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100, // 100 requests
                            Window = TimeSpan.FromMinutes(1) // per minute
                        }));

                // Optional: Customize the response when rate limit is exceeded
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