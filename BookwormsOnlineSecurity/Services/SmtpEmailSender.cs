using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BookwormsOnlineSecurity.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogWarning("SMTP settings are not configured properly.");
                throw new InvalidOperationException("Email service is not configured.");
            }

            MailAddress toAddr;
            try
            {
                toAddr = new MailAddress(to);
            }
            catch
            {
                _logger.LogWarning("Invalid recipient email address provided: {Recipient}", MaskEmail(to));
                throw new InvalidOperationException("Invalid recipient address.");
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);

            using var msg = new MailMessage(from, toAddr)
            {
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };

            try
            {
                await client.SendMailAsync(msg);
                // Log minimal non-sensitive info only
                _logger.LogInformation("Sent email with subject '{Subject}' to {Recipient}", subject, MaskEmail(to));
            }
            catch (SmtpException ex)
            {
                // Log the exception but avoid writing sensitive content such as email body or credentials
                _logger.LogError(ex, "SMTP error while sending email to {Recipient}", MaskEmail(to));
                throw new InvalidOperationException("Failed to send email. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email to {Recipient}", MaskEmail(to));
                throw new InvalidOperationException("Failed to send email. Please try again later.");
            }
        }

        private static string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            try
            {
                var parts = email.Split('@');
                if (parts.Length !=2) return email;
                var local = parts[0];
                var domain = parts[1];
                if (local.Length <=2) local = new string('*', local.Length);
                else local = local.Substring(0,1) + new string('*', Math.Max(1, local.Length -2)) + local[^1];
                return local + "@" + domain;
            }
            catch
            {
                return "***@***";
            }
        }
    }
}
