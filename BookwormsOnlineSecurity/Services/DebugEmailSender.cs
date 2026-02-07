using System.Diagnostics;

namespace BookwormsOnlineSecurity.Services
{
 public class DebugEmailSender : IEmailSender
 {
 public Task SendEmailAsync(string to, string subject, string html)
 {
 Debug.WriteLine($"[EMAIL] To:{to} Subject:{subject} Body:{html}");
 return Task.CompletedTask;
 }
 }
}
