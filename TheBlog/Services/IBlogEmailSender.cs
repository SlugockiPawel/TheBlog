using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace TheBlog.Services
{
    internal interface IBlogEmailSender : IEmailSender
    {
        Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage);
    }
}