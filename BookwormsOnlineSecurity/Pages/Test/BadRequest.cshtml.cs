using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookwormsOnlineSecurity.Pages.Test
{
 public class BadRequestModel : PageModel
 {
 public IActionResult OnGet()
 {
 // Return400 Bad Request to test status code handling
 return BadRequest();
 }
 }
}
