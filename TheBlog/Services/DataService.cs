using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;

namespace TheBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;

        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager,
            UserManager<BlogUser> userManager)
        {
            _dbContext = dbContext; // constructor injection can be done only for registered service (Startup.cs)
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task ManageDataAsync() // wrapper method (method that calls other methods)
        {
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

            // Create a new instance of BlogUser
            var adminUser = new BlogUser()
            {
                Email = "slugocki.pawel@gmail.com",
                UserName = "slugocki.pawel@gmail.com",
                FirstName = "Pawel",
                LastName = "Slugocki",
                PhoneNumber = "123-456-789",
                EmailConfirmed = true,
            };

            // Use the UserManager to create a new user that is defined by adminUser
           await _userManager.CreateAsync(adminUser, "Abc&123!");

           // Add this new user to the Administrator role
           await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());
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