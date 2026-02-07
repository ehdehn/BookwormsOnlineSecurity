using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;

namespace BookwormsOnlineSecurity.Pages.Account
{
 public class Disable2faModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;

 public Disable2faModel(UserManager<ApplicationUser> userManager)
 {
 _userManager = userManager;
 }

 public async Task<IActionResult> OnGetAsync()
 {
 var user = await _userManager.GetUserAsync(User);
 if (user == null) return RedirectToPage("/Login");
 await _userManager.SetTwoFactorEnabledAsync(user, false);
 TempData["StatusMessage"] = "Two-factor authentication has been disabled.";
 return RedirectToPage("/Account/Enable2fa");
 }
 }
}
