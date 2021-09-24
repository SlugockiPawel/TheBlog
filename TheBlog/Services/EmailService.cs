using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

        // method of .net IEmailSender
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new System.NotImplementedException();
        }

        // our custom method
        public Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
