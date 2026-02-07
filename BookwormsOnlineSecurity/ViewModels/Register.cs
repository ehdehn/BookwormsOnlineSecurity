using System.ComponentModel.DataAnnotations;

namespace BookwormsOnlineSecurity.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[A-Za-z ,.'-]{1,50}$", ErrorMessage = "First name contains invalid characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[A-Za-z ,.'-]{1,50}$", ErrorMessage = "Last name contains invalid characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Mobile No")]
        [RegularExpression(@"^[0-9+\-() ]{8,20}$", ErrorMessage = "Mobile number format is invalid.")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing address is required")]
        [Display(Name = "Billing Address")]
        [RegularExpression(@"^[^<>]{1,200}$", ErrorMessage = "Billing address contains invalid characters.")]
        public string BillingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required")]
        [Display(Name = "Shipping Address")]
        [RegularExpression(@"^[^<>]{1,200}$", ErrorMessage = "Shipping address contains invalid characters.")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credit card is required")]
        [CreditCard(ErrorMessage = "Invalid credit card number")]
        [RegularExpression(@"^[0-9]{13,19}$", ErrorMessage = "Credit card number must be 13-19 digits.")]
        [Display(Name = "Credit Card")]
        public string CreditCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 12)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{12,}$", ErrorMessage = "Password must be at least 12 characters and include upper, lower, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Photo is required")]
        public IFormFile Photo { get; set; } = default!;

        [Required(ErrorMessage = "reCAPTCHA validation failed")]
        public string RecaptchaToken { get; set; } = string.Empty;
    }
}
