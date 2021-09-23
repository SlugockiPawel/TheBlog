using System.Threading.Tasks;
using TheBlog.Data;

namespace TheBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext; // constructor injection can be done only for registered service (Startup.cs)
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
            throw new System.NotImplementedException();
        }


       

    }
}
