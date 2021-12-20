using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;
using TheBlog.Services;
using X.PagedList;

namespace TheBlog.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;

        public CategoriesController(ApplicationDbContext context, IImageService imageService,
            UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Categories.Include(b => b.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts by Category
        [AllowAnonymous]
        public async Task<IActionResult> CategoryIndex(int? page, string categoryName)
        {
            ViewData["CategoryName"] = categoryName; // when we go from page 1 to page 2, we will maintain tagText

            var pageNumber = page ?? 1;
            var pageSize = 5;

            var posts = await _context.Posts
                .Include(p => p.BlogUser)
                .Where(p => p.Category.Name.ToLower() == categoryName && p.ReadyStatus == ReadyStatus.ProductionReady)
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

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Categories
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Image")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.Created = DateTime.UtcNow;
                category.BlogUserId = _userManager.GetUserId(User);

                category.ImageData = await _imageService.EncodeImageAsync(category.Image); //IFormFile to byte[]
                category.ContentType = _imageService.ContentType(category.Image);

                _context.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", category.BlogUserId);
            return View(category);
        }

        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Categories.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category, IFormFile newImage)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // alt way
                    // var newBlog = await _context.Blogs.FindAsync(blog.Id);
                    //
                    // newBlog.Updated = DateTime.UtcNow;
                    //
                    // if (newBlog.Name != blog.Name)
                    // {
                    //     newBlog.Name = blog.Name;
                    // }
                    //
                    // if (newBlog.Description != blog.Description)
                    // {
                    //     newBlog.Description = blog.Description;
                    // }
                    //
                    // if (newImage is not null)
                    // {
                    //     newBlog.ImageData = await _imageService.EncodeImageAsync(newImage);
                    // }

                    var currentDbBlog = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    category.Updated = DateTime.UtcNow;
                    category.BlogUserId = _userManager.GetUserId(User);
                    category.Created = currentDbBlog.Created;

                    if (newImage is not null)
                    {
                        category.ImageData = await _imageService.EncodeImageAsync(newImage);
                        category.ContentType = newImage.ContentType;
                    }
                    else
                    {
                        category.ImageData = currentDbBlog.ImageData;
                        category.ContentType = currentDbBlog.ContentType;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(category.Id))
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

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", category.BlogUserId);
            return View(category);
        }

        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Categories
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Categories.FindAsync(id);

            if (blog is null)
            {
                return NotFound();
            }

            if (!User.IsInRole(BlogRole.Administrator.ToString()))
            {
                throw new InvalidOperationException("You are not authorized to delete categories!");
            }

            _context.Categories.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}