using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BookwormsOnlineSecurity.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;
        private const int MaxBodyLength = 4000;
        private const string DefaultSubject = "Bookworms Online Notification";

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
                _logger.LogWarning("Invalid recipient email address provided.");
                throw new InvalidOperationException("Invalid recipient address.");
            }

            // Use a fixed subject and build a controlled, plain-text body.
            var safeSubject = DefaultSubject;
            var link = ExtractFirstHttpLink(html);
            var safeBody = BuildTemplateBody(link);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);

            using var msg = new MailMessage(from, toAddr)
            {
                Subject = safeSubject,
                Body = safeBody,
                IsBodyHtml = false
            };

            try
            {
                await client.SendMailAsync(msg);
                _logger.LogInformation("Email send completed.");
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error while sending email.");
                throw new InvalidOperationException("Failed to send email. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email.");
                throw new InvalidOperationException("Failed to send email. Please try again later.");
            }
        }

        private static string ExtractFirstHttpLink(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            var match = Regex.Match(html, @"https?://[^\s""'<>]+", RegexOptions.IgnoreCase);
            if (!match.Success) return string.Empty;
            var encoded = HtmlEncoder.Default.Encode(match.Value);
            return encoded.Length <= 1000 ? encoded : encoded.Substring(0, 1000);
        }

        private static string BuildTemplateBody(string link)
        {
            var baseText = "If you requested this action, use the link below. If not, ignore this email.";
            if (string.IsNullOrEmpty(link)) return baseText;
            var composed = baseText + "\n\n" + link;
            return composed.Length <= MaxBodyLength ? composed : composed.Substring(0, MaxBodyLength);
        }
    }
}
