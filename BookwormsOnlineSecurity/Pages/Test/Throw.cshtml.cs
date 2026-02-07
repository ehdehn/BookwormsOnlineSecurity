using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookwormsOnlineSecurity.Pages.Test
{
    public class ThrowModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Return a500 status code to exercise the custom error page without throwing an exception
            return StatusCode(500);
        }
    }
}
