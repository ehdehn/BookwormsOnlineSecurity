using System.IO;
using BookwormsOnlineSecurity.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookwormsOnlineSecurity.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataProtector _ccProtector;

        public ApplicationUser? CurrentUser { get; set; }
        public string? SessionLoginTime { get; set; }
        public int? SessionLoginCount { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager, ILogger<IndexModel> logger, IDataProtectionProvider dataProtectionProvider)
        {
            _userManager = userManager;
            _logger = logger;
            _ccProtector = dataProtectionProvider.CreateProtector("CreditCard_v1");
        }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                CurrentUser = await _userManager.GetUserAsync(User);

                if (!string.IsNullOrEmpty(CurrentUser?.CreditCard))
                {
                    var decrypted = TryUnprotectWithFallback(CurrentUser.CreditCard);
                    CurrentUser.CreditCard = decrypted ?? "[Decryption Failed]";
                }
            }

            SessionLoginTime = HttpContext.Session.GetString("LoginTime");
            SessionLoginCount = HttpContext.Session.GetInt32("LoginCount");
        }

        private string? TryUnprotectWithFallback(string cipherText)
        {
            // Primary: current app protector
            try
            {
                return _ccProtector.Unprotect(cipherText);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary decryption failed, attempting legacy protector");
            }

            // Legacy: previous protector/app name with default key store location
            try
            {
                var legacyKeysPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ASP.NET",
                    "DataProtection-Keys");
                var legacyProvider = DataProtectionProvider.Create(new DirectoryInfo(legacyKeysPath), opts =>
                {
                    opts.SetApplicationName("BookwormsData");
                });
                var legacyProtector = legacyProvider.CreateProtector("CreditCard_v1");
                return legacyProtector.Unprotect(cipherText);
            }
            catch (Exception ex2)
            {
                _logger.LogWarning(ex2, "Legacy decryption also failed");
                return null;
            }
        }
    }
}
