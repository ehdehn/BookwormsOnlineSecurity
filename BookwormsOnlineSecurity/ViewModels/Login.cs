using System.ComponentModel.DataAnnotations;

namespace BookwormsOnlineSecurity.ViewModels
{
    public class Login
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "reCAPTCHA validation failed")]
        public string RecaptchaToken { get; set; } = string.Empty;
    }
}
