using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheBlog.Services;

namespace TheBlog;

/*  TO DO LIST:
 * TODO check authorization methods if app is secured
 */
public sealed class Program
{
    public static async Task Main(string[] args)
    {
        // CreateHostBuilder(args).Build().Run();

        var host = CreateHostBuilder(args).Build();

        // Pull out registered DataService
        var dataService = host.Services
            .CreateScope()
            .ServiceProvider.GetRequiredService<DataService>();

        await dataService.ManageDataAsync();

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}