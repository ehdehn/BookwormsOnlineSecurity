using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;

namespace BookwormsOnlineSecurity.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _db;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, AuthDbContext db)
        {
            _signInManager = signInManager;
            _db = db;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
            var email = User?.Identity?.Name;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(email))
            {
                _db.LoginAudits.Add(new LoginAudit
                {
                    UserId = userId,
                    Email = email,
                    Success = true,
                    IsLogout = true,
                    TimestampUtc = DateTime.UtcNow,
                    IpAddress = ip
                });
                await _db.SaveChangesAsync();
            }

            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Login");
        }
    }
}
