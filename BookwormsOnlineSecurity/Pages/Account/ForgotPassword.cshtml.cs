using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookwormsOnlineSecurity.Models;
using BookwormsOnlineSecurity.Services;
using System.ComponentModel.DataAnnotations;

namespace BookwormsOnlineSecurity.Pages.Account
{
 public class ForgotPasswordModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly IEmailSender _email;

 public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender email)
 {
 _userManager = userManager;
 _email = email;
 }

 [BindProperty]
 public InputModel Input { get; set; } = new InputModel();

 public class InputModel
 {
 [Required]
 [EmailAddress]
 public string Email { get; set; } = string.Empty;
 }

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 var user = await _userManager.FindByEmailAsync(Input.Email);
 if (user == null) return RedirectToPage("/Account/ForgotPasswordConfirmation");
 var token = await _userManager.GeneratePasswordResetTokenAsync(user);
 var callback = Url.Page("/Account/ResetPassword", null, new { userId = user.Id, token }, Request.Scheme);
 await _email.SendEmailAsync(user.Email!, "Reset Password", $"Click here to reset: <a href=\"{callback}\">link</a>");
 return RedirectToPage("/Account/ForgotPasswordConfirmation");
 }
 }
}
