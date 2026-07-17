using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
    {
        protected readonly ILogger<T> _logger;

        public BaseController(ILogger<T> logger)
        {
            _logger = logger;
        }
    }
}