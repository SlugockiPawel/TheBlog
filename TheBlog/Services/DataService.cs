using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;

namespace TheBlog.Services
{
    public sealed class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IConfiguration _configuration;

        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager,
            UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext; // constructor injection can be done only for registered service (Startup.cs)
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task ManageDataAsync() // wrapper method (method that calls other methods)
        {
            // Create the DB from the Migrations
            await _dbContext.Database.MigrateAsync();

            //Task 1: Seed a few Roles into the system
            await SeedRolesAsync();

            //Task 2: Seed a few users into the system 
            await SeedUsersAsync();
        }

        private async Task SeedUsersAsync()
        {
            // If Users are already in the system, do nothing
            // if they are not, add them to the database

            if (_dbContext.Users.Any())
            {
                return;
            }

            var admin = _configuration.GetSection("AppOwner");
            var moderator = _configuration.GetSection("DefaultModerator");

            // Create a new instance of BlogUser
            var adminUser = new BlogUser()
            {
                Email = admin["Email"],
                UserName = admin["Email"],
                FirstName = admin["FirstName"],
                LastName = admin["LastName"],
                EmailConfirmed = true,
            };

            // Use the UserManager to create a new user that is defined by adminUser
            await _userManager.CreateAsync(adminUser, admin["Password"]);

            // Add this new user to the Administrator role
            await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());

            // Add moderator user:
            var modUser = new BlogUser()
            {
                Email = moderator["Email"],
                UserName = moderator["Email"],
                FirstName = moderator["FirstName"],
                LastName = moderator["LastName"],
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(modUser, moderator["Password"]);
            await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());
        }

        private async Task SeedRolesAsync()
        {
            // If Roles are already in the system, do nothing
            // if they are not, add them to the database

            if (_dbContext.Roles.Any())
            {
                return;
            }

            // Otherwise, create set of roles
            foreach (var role in Enum.GetNames(typeof(BlogRole)))
            {
                // need to use Role manager to create roles
                var newRole = await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}