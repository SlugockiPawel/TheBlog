using TheBlog.Data;

namespace TheBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Task 1: Seed a few Roles into the system

        //Task 2: Seed a few users into the system 

    }
}
