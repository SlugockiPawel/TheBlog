
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TheBlog.ViewModels;

namespace TheBlog.Services
{
    public class EmailService : IBlogEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings) //IOptions as EmailService is preconfigured 
        {
            _emailSettings = emailSettings.Value;
        }

        // our custom method
        public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_emailSettings.Email)
            };

            email.To.Add(MailboxAddress.Parse(_emailSettings.Email));
            email.Subject = subject;

            var builder = new BodyBuilder();            
            builder.HtmlBody = $"<b>{name}</b> has sent you an email and can be reached at: <b>{emailFrom}</b><br/><br/>{htmlMessage}";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
 
            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }

        // method of .net IEmailSender
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_emailSettings.Email)
            };

            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder()
            {
                HtmlBody = htmlMessage,
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}