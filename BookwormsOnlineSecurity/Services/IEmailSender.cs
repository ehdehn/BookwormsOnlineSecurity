namespace BookwormsOnlineSecurity.Services
{
    public interface IEmailSender
    {
        Task SendPasswordResetAsync(string to, string resetLink);
    }
}
