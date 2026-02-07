using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;
using System.ComponentModel.DataAnnotations;

namespace BookwormsOnlineSecurity.Pages.Account
{
 public class ResetPasswordModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly AuthDbContext _db;

 public ResetPasswordModel(UserManager<ApplicationUser> userManager, AuthDbContext db)
 {
 _userManager = userManager;
 _db = db;
 }

 [BindProperty]
 public InputModel Input { get; set; } = new InputModel();

 public class InputModel
 {
 [Required]
 public string UserId { get; set; } = string.Empty;
 [Required]
 public string Token { get; set; } = string.Empty;
 [Required]
 [DataType(DataType.Password)]
 public string NewPassword { get; set; } = string.Empty;
 }

 public void OnGet(string userId, string token)
 {
 Input.UserId = userId;
 Input.Token = token;
 }

 public async Task<IActionResult> OnPostAsync()
 {
 var user = await _userManager.FindByIdAsync(Input.UserId);
 if (user == null) return RedirectToPage("/Account/ResetPasswordConfirmation");
 var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.NewPassword);
 if (!result.Succeeded)
 {
 foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
 return Page();
 }

 // Save password history
 _db.PasswordHistories.Add(new PasswordHistory { UserId = user.Id, PasswordHash = user.PasswordHash });
 await _db.SaveChangesAsync();

 return RedirectToPage("/Account/ResetPasswordConfirmation");
 }
 }
}
