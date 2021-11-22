using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TheBlog.Services;

namespace TheBlog
{
    /*  TO DO LIST:
     *  TODO implement Home/Index logic to display: Post list (sorted by date)
     *  TODO implement Home/Index logic to display posts by category and by tag
     *  TODO authorize access to /Blogs and /Posts routes (only admin and moderator)
     */
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // CreateHostBuilder(args).Build().Run();

            var host = CreateHostBuilder(args).Build();

            // Pull out registered DataService
            var dataService = host.Services.CreateScope().ServiceProvider.GetRequiredService<DataService>();

            await dataService.ManageDataAsync();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}