using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PresupuestosAPI.Settings;

namespace PresupuestosAPI.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            ValidateSettings();

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();

            var secureSocketOption = _settings.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtpClient.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                secureSocketOption
            );

            if (!string.IsNullOrWhiteSpace(_settings.SmtpUser))
            {
                await smtpClient.AuthenticateAsync(
                    _settings.SmtpUser,
                    _settings.SmtpPassword
                );
            }

            await smtpClient.SendAsync(message);

            await smtpClient.DisconnectAsync(true);
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.FromName))
            {
                throw new InvalidOperationException("EmailSettings:FromName no está configurado.");
            }

            if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                throw new InvalidOperationException("EmailSettings:FromEmail no está configurado.");
            }

            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                throw new InvalidOperationException("EmailSettings:SmtpHost no está configurado.");
            }

            if (_settings.SmtpPort <= 0)
            {
                throw new InvalidOperationException("EmailSettings:SmtpPort no está configurado correctamente.");
            }
        }
    }
}