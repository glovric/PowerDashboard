using System.ComponentModel.DataAnnotations;

namespace AuthService.Data.Seed
{
    public class SuperUserSettings
    {
        [Required(ErrorMessage = "SuperUser UserName cannot be empty! Make sure you set a value in settings.")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "SuperUser Email cannot be empty! Make sure you set a value in settings.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "SuperUser Password cannot be empty! Make sure you set a value in settings.")]
        public string Password { get; set; } = string.Empty;
    }
}
