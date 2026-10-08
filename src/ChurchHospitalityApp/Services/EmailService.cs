using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ChurchHospitalityApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
        {
            var emailConfig = _config.GetSection("Email");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailConfig["FromName"] ?? "Church Hospitality Team",
                emailConfig["FromAddress"]));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var host = emailConfig["SmtpHost"];
                var port = int.Parse(emailConfig["SmtpPort"] ?? "587");
                var useStartTls = bool.Parse(emailConfig["UseStartTls"] ?? "true");

                await client.ConnectAsync(host, port,
                    useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                var username = emailConfig["Username"];
                if (!string.IsNullOrWhiteSpace(username))
                {
                    await client.AuthenticateAsync(username, emailConfig["Password"]);
                }

                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public async Task SendMemberInviteAsync(string toAddress, string toName, string completeRegistrationUrl)
        {
            // Encode user-supplied values before placing them in HTML
            var safeName = WebUtility.HtmlEncode(toName);
            var safeUrl = WebUtility.HtmlEncode(completeRegistrationUrl);

            var subject = "You've been added to the Church Hospitality App";
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 560px; margin: auto;'>
                    <h2 style='color:#7a3e2b;'>Welcome, {safeName}!</h2>
                    <p>You've been added as a member of the Hospitality Unit on our church's guest
                    follow-up app. To get started, please confirm your email and set a password:</p>
                    <p style='text-align:center; margin: 24px 0;'>
                        <a href='{safeUrl}'
                           style='background:#7a3e2b; color:#fff; padding:12px 24px; border-radius:6px;
                                  text-decoration:none; font-weight:bold;'>
                            Complete My Registration
                        </a>
                    </p>
                    <p>If the button doesn't work, copy and paste this link into your browser:<br>
                    <a href='{safeUrl}'>{safeUrl}</a></p>
                    <p style='color:#888; font-size:12px;'>If you weren't expecting this, you can ignore this email.</p>
                </div>";

            try
            {
                await SendAsync(toAddress, toName, subject, html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send member invite email to {Email}", toAddress);
                throw;
            }
        }
    }
}
