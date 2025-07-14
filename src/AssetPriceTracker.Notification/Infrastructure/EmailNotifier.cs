using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using AssetPriceTracker.Notification.Application.Services;

namespace AssetPriceTracker.Notification.Infrastructure
{
    public class EmailNotifier : INotificationService
    {
        private readonly ILogger<EmailNotifier> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly bool _enableSsl;

        public EmailNotifier(
            ILogger<EmailNotifier> logger,
            string smtpHost,
            int smtpPort,
            string fromEmail,
            string fromPassword,
            bool enableSsl = true
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smtpHost = smtpHost ?? throw new ArgumentNullException(nameof(smtpHost));
            _smtpPort = smtpPort;
            _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
            _fromPassword = fromPassword ?? throw new ArgumentNullException(nameof(fromPassword));
            _enableSsl = enableSsl;
        }

        public async Task Notify(string toEmail, string subject, string message)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                    EnableSsl = _enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                _logger.LogInformation($"To email: {toEmail}");
                _logger.LogInformation($"Subject: {subject}");
                _logger.LogInformation($"Message: {message}");

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP server is unreachable or failed to send email.");
                throw new InvalidOperationException("Failed to send notification: SMTP service may be unavailable.", smtpEx);
            }
        }
    }
}
