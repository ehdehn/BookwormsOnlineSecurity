using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookwormsOnlineSecurity.Pages.Account
{
 public class ChangePasswordModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly AuthDbContext _db;
 private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

 public ChangePasswordModel(UserManager<ApplicationUser> userManager, AuthDbContext db, IPasswordHasher<ApplicationUser> passwordHasher)
 {
 _userManager = userManager;
 _db = db;
 _passwordHasher = passwordHasher;
 }

 [BindProperty]
 public InputModel Input { get; set; } = new InputModel();

 public class InputModel
 {
 [Required]
 [DataType(DataType.Password)]
 public string CurrentPassword { get; set; } = string.Empty;

 [Required]
 [DataType(DataType.Password)]
 public string NewPassword { get; set; } = string.Empty;
 }

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 var user = await _userManager.GetUserAsync(User);
 if (user == null) return RedirectToPage("/Login");

 // Check minimum password age (e.g.,5 minutes)
 var lastChange = await _db.PasswordHistories.OrderByDescending(p => p.CreatedUtc).FirstOrDefaultAsync(p => p.UserId == user.Id);
 if (lastChange != null && DateTime.UtcNow - lastChange.CreatedUtc < TimeSpan.FromMinutes(5))
 {
 ModelState.AddModelError(string.Empty, "Cannot change password yet. Minimum password age not met.");
 return Page();
 }

 // Prevent choosing the same password as current
 if (await _userManager.CheckPasswordAsync(user, Input.NewPassword))
 {
 ModelState.AddModelError(string.Empty, "New password must be different from the current password.");
 return Page();
 }

 // Prevent reuse of recent passwords (last2)
 var histories = await _db.PasswordHistories.Where(p => p.UserId == user.Id).OrderByDescending(p => p.CreatedUtc).Take(2).ToListAsync();
 foreach (var h in histories)
 {
 var verify = _passwordHasher.VerifyHashedPassword(user, h.PasswordHash, Input.NewPassword);
 if (verify == PasswordVerificationResult.Success)
 {
 ModelState.AddModelError(string.Empty, "You cannot reuse your recent passwords.");
 return Page();
 }
 }

 // Save current password hash to history BEFORE changing
 _db.PasswordHistories.Add(new PasswordHistory { UserId = user.Id, PasswordHash = user.PasswordHash, CreatedUtc = DateTime.UtcNow });
 await _db.SaveChangesAsync();

 var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
 if (!result.Succeeded)
 {
 foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
 return Page();
 }

 // Trim history to last2 entries
 var allHist = await _db.PasswordHistories.Where(p => p.UserId == user.Id).OrderByDescending(p => p.CreatedUtc).ToListAsync();
 if (allHist.Count >2)
 {
 _db.PasswordHistories.RemoveRange(allHist.Skip(2));
 await _db.SaveChangesAsync();
 }

 return RedirectToPage("/Index");
 }
 }
}
