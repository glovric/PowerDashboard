using Microsoft.AspNetCore.Mvc;

namespace PowerService.Controllers
{
    public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
    {
        protected readonly ILogger<T> _logger;

        public readonly HashSet<string> allowedCountriesHour = new(StringComparer.OrdinalIgnoreCase) 
        { "AT", "BE", "BG", "CH", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR", "GB", "GR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "ME", "NL", "NO", "PL", "PT", "RO", "RS", "SE", "SI", "SK", "UA" };

        public readonly HashSet<string> allowedCountriesQuarter = new(StringComparer.OrdinalIgnoreCase) 
        { "AT", "BE", "DE", "HU", "LU", "NL" };

        public BaseController(ILogger<T> logger)
        {
            _logger = logger;
        }
    }
}