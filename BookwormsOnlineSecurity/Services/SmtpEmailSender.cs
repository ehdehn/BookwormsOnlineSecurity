using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace BookwormsOnlineSecurity.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailSender(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
            {
                throw new InvalidOperationException("SMTP is not configured. Please set Smtp settings in configuration.");
            }

            // For Brevo: Host=smtp-relay.brevo.com, Port=587, UseSsl=true, User=SMTP login, Password=SMTP API key
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);
            var toAddr = new MailAddress(to);

            using var msg = new MailMessage(from, toAddr)
            {
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };

            await client.SendMailAsync(msg);
        }
    }
}
