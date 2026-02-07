using BookwormsOnlineSecurity.Models;
using BookwormsOnlineSecurity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Services;
using Microsoft.Extensions.Options;

namespace BookwormsOnlineSecurity.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _db;
        private readonly IRecaptchaValidator _recaptcha;
        private readonly RecaptchaSettings _recaptchaSettings;

        [BindProperty]
        public Login LModel { get; set; } = default!;

        public string RecaptchaSiteKey => _recaptchaSettings.SiteKey;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext db, IRecaptchaValidator recaptcha, IOptions<RecaptchaSettings> recaptchaOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _recaptcha = recaptcha;
            _recaptchaSettings = recaptchaOptions.Value;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await ValidateRecaptchaAsync())
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA validation failed. Please try again.");
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    LModel.Email,
                    LModel.Password,
                    LModel.RememberMe,
                    lockoutOnFailure: true);

                var user = await _userManager.FindByEmailAsync(LModel.Email);
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (user != null)
                {
                    _db.LoginAudits.Add(new LoginAudit
                    {
                        UserId = user.Id,
                        Email = LModel.Email,
                        Success = result.Succeeded,
                        IsLogout = false,
                        TimestampUtc = DateTime.UtcNow,
                        IpAddress = ip
                    });
                    await _db.SaveChangesAsync();
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginWith2fa", new { rememberMe = LModel.RememberMe });
                }

                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        var sessionId = Guid.NewGuid().ToString("N");
                        user.LastSessionId = sessionId;
                        await _userManager.UpdateAsync(user);

                        HttpContext.Session.SetString("UserId", user.Id);
                        HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);
                        HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));
                        HttpContext.Session.SetInt32("LoginCount", 1);
                        HttpContext.Session.SetString("SessionId", sessionId);
                    }
                    return RedirectToPage("/Index");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked due to repeated failures. Try again later.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            return Page();
        }

        private async Task<bool> ValidateRecaptchaAsync()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return await _recaptcha.IsValidAsync(LModel.RecaptchaToken, "login", remoteIp);
        }
    }
}
