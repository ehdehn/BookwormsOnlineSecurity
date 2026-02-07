using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;
using BookwormsOnlineSecurity.ViewModels;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using BookwormsOnlineSecurity.Services;
using System.Text.Encodings.Web;

namespace BookwormsOnlineSecurity.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IDataProtector _ccProtector;
        private readonly IRecaptchaValidator _recaptcha;
        private readonly RecaptchaSettings _recaptchaSettings;
        private readonly HtmlEncoder _encoder;

        [BindProperty]
        public Register RModel { get; set; } = default!;

        public string RecaptchaSiteKey => _recaptchaSettings.SiteKey;

        public RegisterModel(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             IWebHostEnvironment environment,
                             IDataProtectionProvider dataProtectionProvider,
                             IRecaptchaValidator recaptcha,
                             IOptions<RecaptchaSettings> recaptchaOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _ccProtector = dataProtectionProvider.CreateProtector("CreditCard_v1");
            _recaptcha = recaptcha;
            _recaptchaSettings = recaptchaOptions.Value;
            _encoder = HtmlEncoder.Default;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Debug.WriteLine("=== POST HIT! ===");

            if (!await ValidateRecaptchaAsync())
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA validation failed. Please try again.");
            }

            // Check duplicate email early
            var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("RModel.Email", "Email is already registered.");
            }

            if (ModelState.IsValid)
            {
                Debug.WriteLine($"FirstName: '{RModel.FirstName}' Email: '{RModel.Email}' CreditCard: '{RModel.CreditCard}'");

                // Handle photo upload
                string? photoPath = null;
                if (RModel.Photo != null && RModel.Photo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{RModel.Photo.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await RModel.Photo.CopyToAsync(fileStream);
                    }

                    photoPath = $"/uploads/{uniqueFileName}";
                }

                var encryptedCard = _ccProtector.Protect(RModel.CreditCard);

                var user = new ApplicationUser
                {
                    UserName = _encoder.Encode(RModel.Email),
                    Email = _encoder.Encode(RModel.Email),
                    EmailConfirmed = true,
                    FirstName = _encoder.Encode(RModel.FirstName),
                    LastName = _encoder.Encode(RModel.LastName),
                    MobileNo = _encoder.Encode(RModel.MobileNo),
                    BillingAddress = _encoder.Encode(RModel.BillingAddress),
                    ShippingAddress = _encoder.Encode(RModel.ShippingAddress),
                    CreditCard = encryptedCard,
                    PhotoPath = photoPath ?? string.Empty
                };

                var result = await _userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    Debug.WriteLine("USER CREATED SUCCESSFULLY!");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("/Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Debug.WriteLine($"ERROR: {error.Description}");
                }
            }
            else
            {
                Debug.WriteLine("ModelState INVALID");
                foreach (var error in ModelState)
                {
                    Debug.WriteLine($"Validation Error: {error.Key} = {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            return Page();
        }

        private async Task<bool> ValidateRecaptchaAsync()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return await _recaptcha.IsValidAsync(RModel.RecaptchaToken, "register", remoteIp);
        }
    }
}
