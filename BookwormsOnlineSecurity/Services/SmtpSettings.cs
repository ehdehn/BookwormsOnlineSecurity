namespace BookwormsOnlineSecurity.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string User { get; set; } = string.Empty; // For Brevo, this is your SMTP login
        public string Password { get; set; } = string.Empty; // For Brevo, this is your SMTP API key
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Bookworms Online";
    }
}
