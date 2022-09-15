using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheBlog.Data;
using TheBlog.Models;
using TheBlog.Services;
using TheBlog.ViewModels;

namespace TheBlog;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlServer(
        // Configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseNpgsql(ConnectionService.GetConnectionString(Configuration))
        );
        // Configuration.GetConnectionString("PostgresConnection")));

        services.AddDatabaseDeveloperPageExceptionFilter();

        // services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        //     .AddEntityFrameworkStores<ApplicationDbContext>();
        services
            .AddIdentity<BlogUser, IdentityRole>(
                options => options.SignIn.RequireConfirmedAccount = true
            )
            .AddDefaultUI()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddControllersWithViews();
        services.AddRazorPages();

        //google Auth
        services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                var googleAuthNSection = Configuration.GetSection(
                    "Authentication:Google"
                );

                options.ClientId = googleAuthNSection["ClientId"];
                options.ClientSecret = googleAuthNSection["ClientSecret"];
            });

        // Register custom DataService class
        services.AddScoped<DataService>();

        // Register BlogSearchService
        services.AddScoped<BlogSearchService>();

        //Register a preconfigured instance of the EmailSettings class
        //Will populate an instance of EmailSettings with values from EmailSettings in appsettings.json
        services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));

        services.AddScoped<IBlogEmailSender, EmailService>();

        // Register ImageService
        services.AddScoped<IImageService, BasicImageService>();

        // Register SlugService
        services.AddScoped<ISlugService, BasicSlugService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "SlugRoute",
                "BlogPosts/UrlFriendly/{slug}",
                new { controller = "Posts", action = "Details" }
            );

            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}"
            );

            endpoints.MapRazorPages();
        });
    }
}