using System.Linq;
using System.Text;
using TheBlog.Data;

namespace TheBlog.Services
{
    public class BasicSlugService : ISlugService
    {
        private readonly ApplicationDbContext _dbContext;

        public BasicSlugService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string UrlFriendly(string title)
        {
            throw new System.NotImplementedException();
        }

        public bool IsUnique(string slug)
        {
            throw new System.NotImplementedException();
        }
    }
}
