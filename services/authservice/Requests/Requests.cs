using System.ComponentModel.DataAnnotations;

namespace AuthService.Requests {

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email cannot be empty")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; } = string.Empty;
    }

}