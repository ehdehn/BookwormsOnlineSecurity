using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BookwormsOnlineSecurity.Models;

namespace BookwormsOnlineSecurity.Pages.Account
{
    public class Enable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UrlEncoder _urlEncoder;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private readonly string _issuer;

        public Enable2faModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _urlEncoder = UrlEncoder.Default;
            _issuer = "BookwormsOnline";
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [Display(Name = "Verification Code")]
            public string Code { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["StatusMessage"] = "Two-factor authentication has been enabled.";
            return RedirectToPage("/Index");
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);
            var email = user.Email ?? user.UserName ?? "user";
            AuthenticatorUri = GenerateQrCodeUri(_issuer, email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            for (int i = 0; i < unformattedKey.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    result.Append(' ');
                }
                result.Append(char.ToLowerInvariant(unformattedKey[i]));
            }
            return result.ToString();
        }

        private string GenerateQrCodeUri(string issuer, string email, string unformattedKey)
        {
            return string.Format(AuthenticatorUriFormat,
                _urlEncoder.Encode(issuer),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
