using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;
using TheBlog.Services;
using TheBlog.ViewModels;
using X.PagedList;

namespace TheBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly BlogSearchService _blogSearchService;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender,
            ApplicationDbContext context, BlogSearchService blogSearchService)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _blogSearchService = blogSearchService;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 5;

            var posts = await _context.Posts
                .Include(p => p.BlogUser)
                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewData["DistinctTags"] = await _blogSearchService.GetDistinctTags(15);
            ViewData["Categories"] = await _blogSearchService.GetDistinctCategories();

            return View(posts);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            // send contact me email
            model.Message = $"{model.Message} <hr/> Phone: {model.Phone}";
            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}