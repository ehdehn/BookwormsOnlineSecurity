using System.Diagnostics;

namespace BookwormsOnlineSecurity.Services
{
    public class DebugEmailSender : IEmailSender
    {
        public Task SendPasswordResetAsync(string to, string resetLink)
        {
            Debug.WriteLine($"[EMAIL] To:{to} ResetLink:{resetLink}");
            return Task.CompletedTask;
        }
    }
}
