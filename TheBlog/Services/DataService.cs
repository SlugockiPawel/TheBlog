using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TheBlog.Data;
using TheBlog.Enums;

namespace TheBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext; // constructor injection can be done only for registered service (Startup.cs)
            _roleManager = roleManager;
        }

        public async Task ManageDataAsync() // wrapper class
        {
            //Task 1: Seed a few Roles into the system
            await SeedRolesAsync();

            //Task 2: Seed a few users into the system 
            await SeedUsersAsync();
        }

        private async Task SeedUsersAsync()
        {
            throw new System.NotImplementedException();
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
                var newRole =  await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }


       

    }
}
