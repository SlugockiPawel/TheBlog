using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;
using TheBlog.Services;
using X.PagedList;

namespace TheBlog.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;

        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService,
            UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }

        //GET: Search Posts
        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm; // when we go from page 1 to page 2, we will maintain this search term

            var pageNumber = page ?? 1;
            var pageSize = 5;
            var posts = _blogSearchService.Search(searchTerm);

            if (posts == null)
            {
                return NotFound();
            }

            ViewData["DistinctTags"] = await _blogSearchService.GetDistinctTags(15);
            ViewData["Categories"] = await _blogSearchService.GetDistinctCategories();

            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Posts by Tag
        public async Task<IActionResult> TagIndex(int? page, string tagText)
        {
            ViewData["TagText"] = tagText; // when we go from page 1 to page 2, we will maintain tagText

            var pageNumber = page ?? 1;
            var pageSize = 5;

            var posts = await _context.Posts
                .Include(p => p.BlogUser)
                .Where(p => p.Tags.Any(t => t.Text.ToLower() == tagText) && p.ReadyStatus == ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            if (posts == null)
            {
                return NotFound();
            }

            ViewData["DistinctTags"] = await _blogSearchService.GetDistinctTags(15);
            ViewData["Categories"] = await _blogSearchService.GetDistinctCategories();

            return View(posts);
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts/Details/5
        // public async Task<IActionResult> Details(int? id)
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser) // this BlogUser is an author of the Post (below is for comment)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogUser) // will query above Comments only
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }

            ViewData["DistinctTags"] = await _blogSearchService.GetDistinctTags(15);
            ViewData["Categories"] = await _blogSearchService.GetDistinctCategories();

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            ViewData["BlogUserId"] = new SelectList(_context.Blogs, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus,Image")] Post post,
            List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.UtcNow;

                var authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                // use imageService to store incoming user specified image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);

                // Create a slug and check if unique
                var slug = _slugService.UrlFriendly(post.Title);
                var slugValidationError = false;

                if (string.IsNullOrWhiteSpace(slug))
                {
                    slugValidationError = true;
                    // Add a model state error and return user back to Create view
                    ModelState.AddModelError("Title",
                        "Title cannot be empty. Try again with a different Post Title");
                }

                if (!_slugService.IsUnique(slug))
                {
                    slugValidationError = true;
                    // Add a model state error and return user back to Create view
                    ModelState.AddModelError("Title",
                        "Title provided cannot be used as it is already in the database. Try again with a different Post Title");
                }

                if (slugValidationError)
                {
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
                    return View(post);
                }

                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();

                // add list of Tags to the post
                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        BlogUserId = authorId,
                        PostId = post.Id,
                        Text = tagText,
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            var tagsToDisplay = post.Tags.Select(t => t.Text);
            ViewData["TagValues"] = string.Join(",", tagsToDisplay);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post,
            IFormFile newImage, List<string> tagValues) //name of the select list is tagValues => it has to match!
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentDbPost = await _context.Posts
                        .Include(p => p.Tags)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    post.Created = currentDbPost.Created;
                    post.Updated = DateTime.UtcNow;
                    post.BlogUserId = _userManager.GetUserId(User);
                    post.Slug = currentDbPost.Slug;

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    if (newSlug != currentDbPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            post.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title",
                                "Title provided cannot be used as it is already in the database. Try again with a different Post Title");
                            var tagsToDisplay = currentDbPost.Tags.Select(t => t.Text);
                            ViewData["TagValues"] = string.Join(",", tagsToDisplay);
                            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        post.ImageData = await _imageService.EncodeImageAsync(newImage);
                        post.ContentType = newImage.ContentType;
                    }
                    else
                    {
                        post.ImageData = currentDbPost.ImageData;
                        post.ContentType = currentDbPost.ContentType;
                    }

                    _context.Update(post);
                    await _context.SaveChangesAsync();

                    _context.Entry(post).State = EntityState.Detached;


                    //Remove all Tags previously associated with this Post

                    _context.Tags.RemoveRange(currentDbPost.Tags);


                    //Add new tags to the post from the Edit form
                    foreach (var tagText in tagValues)
                    {
                        _context.Tags.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogUserId = currentDbPost.BlogUserId,
                            Text = tagText,
                        });
                    }

                    // _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}