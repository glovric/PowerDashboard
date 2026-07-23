using Shared;

namespace AuthService.Extensions {

    public static class JwtExtension
    {
        public static IServiceCollection InjectJwtSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>("Frontend", configuration.GetSection("FrontJwtSettings"));
            services.Configure<JwtSettings>("Service", configuration.GetSection("ServiceJwtSettings"));
            return services;
        }
    }

}