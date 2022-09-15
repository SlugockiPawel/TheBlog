using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(TheBlog.Areas.Identity.IdentityHostingStartup))]
namespace TheBlog.Areas.Identity
{
    public sealed class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}