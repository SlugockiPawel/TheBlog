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
     * TODO TagIndex implementation
     * TODO implement category click on post to list all posts with this category logic
     * TODO refactor getting tags to work as a service as it will be used in multiply places
     *  TODO refactor getting categories to work as a service as it will be used in multiply places
     *  
     *
     *
     *  TODO Comments/Details not working
     *  
     *
     *  TODO Render correct NotFound page (currently it is blank)
     *  TODO implement Home/Index logic to display: Post list (sorted by date)
     *  TODO implement Home/Index logic to display posts by category and by tag
     *  TODO authorize access to /Blogs and /Posts routes (only admin and moderator)
     *  TODO in Blogs controller there is a BlogUser hard coded, change it
     *
     *
     * TODO MIGRATE TO .NET6
     * TODO DistinctBy() in .NET6 => use for getting distinct Tags
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