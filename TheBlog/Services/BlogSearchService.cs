using System.Collections.Generic;
using System.Linq;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;
using X.PagedList;

namespace TheBlog.Services
{
    public class BlogSearchService
    {
        private readonly ApplicationDbContext _context;
        public BlogSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Post> Search(string searchTerm)
        {
            var posts = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.BlogUser)
                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).AsQueryable();

            if (searchTerm is not null)
            {
                searchTerm = searchTerm.ToLower();

                posts = posts.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) ||
                    p.Abstract.ToLower().Contains(searchTerm) ||
                    p.Content.ToLower().Contains(searchTerm) ||
                    // p.Tags.Any(t => t.Text.ToLower().Contains(searchTerm)) ||
                    p.Comments.Any(c =>
                        c.Body.ToLower().Contains(searchTerm) ||
                        c.ModeratedBody.ToLower().Contains(searchTerm) ||
                        c.BlogUser.FirstName.ToLower().Contains(searchTerm) ||
                        c.BlogUser.LastName.ToLower().Contains(searchTerm) ||
                        c.BlogUser.Email.ToLower().Contains(searchTerm))
                );
            }

            return posts.OrderByDescending(p => p.Created);
        }

        public async Task<IList<Tag>> GetDistinctTags(int numberOfTags)
        {

            return await _context.Tags
                .Where(t => t.Post.ReadyStatus == ReadyStatus.ProductionReady)
                .OrderByDescending(t => t.Post.Created)
                .AsEnumerable()
                .DistinctBy(t => t.Text)
                .Take(numberOfTags)
                .ToListAsync();
        }

        public async Task<IList<Category>> GetDistinctCategories()
        {
            return await _context.Categories
                .Where(c => c.Posts.Any(p => p.ReadyStatus == ReadyStatus.ProductionReady))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
