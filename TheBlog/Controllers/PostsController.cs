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
using TheBlog.Data;
using TheBlog.Models;
using TheBlog.Services;

namespace TheBlog.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;

        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService,
            UserManager<BlogUser> userManager)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        //BlogPostIndex
        public async Task<IActionResult> BlogPostIndex(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var posts = await _context.Posts.Where(p => p.Id == id).ToListAsync();

            return View("Index", posts);
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
                .Include(p => p.BlogUser)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if (post == null)
            {
                return NotFound();
            }

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
                    post.Updated = DateTime.UtcNow;

                    var currentDbPost = await _context.Posts
                        .Include(p => p.Tags)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    if (newSlug != currentDbPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            post.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "Title provided cannot be used as it is already in the database. Try again with a different Post Title");
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