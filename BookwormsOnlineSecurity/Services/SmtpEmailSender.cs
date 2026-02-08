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
        private const int MaxBodyLength = 2000;
        private const string DefaultSubject = "Bookworms Online - Password Reset";

        public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendPasswordResetAsync(string to, string resetLink)
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

            // Validate and constrain the reset link: must be absolute HTTP/HTTPS and not too long
            if (!Uri.TryCreate(resetLink, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                _logger.LogWarning("Rejected reset link: not an absolute HTTP/HTTPS URI.");
                throw new InvalidOperationException("Invalid reset link.");
            }

            var safeLink = uri.GetLeftPart(UriPartial.Path) + uri.Query + uri.Fragment;
            if (safeLink.Length > 1500)
            {
                _logger.LogWarning("Rejected reset link: too long.");
                throw new InvalidOperationException("Invalid reset link.");
            }

            var body = BuildBody(safeLink);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);

            using var msg = new MailMessage(from, toAddr)
            {
                Subject = DefaultSubject,
                Body = body,
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

        private static string BuildBody(string link)
        {
            var baseText = "If you requested a password reset, use the link below. If not, ignore this email.";
            var composed = string.IsNullOrEmpty(link) ? baseText : baseText + "\n\n" + link;
            return composed.Length <= MaxBodyLength ? composed : composed.Substring(0, MaxBodyLength);
        }
    }
}
