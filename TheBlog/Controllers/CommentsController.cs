using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;

namespace TheBlog.Controllers;

public sealed class CommentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<BlogUser> _userManager;

    public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Comments
    [Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> Index()
    {
        var originalComments = await _context.Comments.ToListAsync();
        return View("Index", originalComments);
    }

    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> ModeratedIndex()
    {
        var moderatedComments = await _context.Comments
            .Where(c => c.Moderated != null)
            .ToListAsync();
        return View("Index", moderatedComments);
    }

    // POST: Comments/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(
        [Bind("PostId,Body")] Comment comment,
        string postSlug
    )
    {
        if (ModelState.IsValid)
        {
            comment.BlogUserId = _userManager.GetUserId(User);
            comment.Created = DateTime.UtcNow;
            _context.Add(comment);
            await _context.SaveChangesAsync();
            // return RedirectToAction(nameof(Index));
            return RedirectToAction(
                "Details",
                "Posts",
                new { slug = postSlug },
                "commentSection"
            );
        }

        TempData["Error"] = GetFirstModelStateErrorMessage();

        return RedirectToAction("Details", "Posts", new { slug = postSlug }, "commentSection");
    }

    [Authorize(Roles = "Moderator")]
    // GET: Comments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
        ViewData["ModeratorId"] = new SelectList(
            _context.Users,
            "Id",
            "Id",
            comment.ModeratorId
        );
        ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
        return View(comment);
    }

    // POST: Comments/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Body")] Comment comment,
        string postSlug
    )
    {
        if (id != comment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var newComment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            try
            {
                newComment.Body = comment.Body;
                newComment.Updated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(
                "Details",
                "Posts",
                new { slug = newComment.Post.Slug },
                "commentSection"
            ); // in View, there is an id for every comment <h4> so after editing, View will stop right there
            // return RedirectToAction("Details", "Posts", new {slug = newComment.Post.Slug}, $"commentNumber_{comment.Id}"); // in View, there is an id for every comment <h4> so after editing, View will stop right there
        }

        TempData["Error"] = GetFirstModelStateErrorMessage();

        return RedirectToAction("Details", "Posts", new { slug = postSlug }, "commentSection");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Moderate(
        int id,
        [Bind("Id,Body,ModeratedBody,ModerationType")]
        Comment comment,
        string postSlug
    )
    {
        if (id != comment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var newComment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            try
            {
                newComment.ModeratedBody = comment.ModeratedBody;
                newComment.ModerationType = comment.ModerationType;
                newComment.Moderated = DateTime.UtcNow;
                newComment.ModeratorId = _userManager.GetUserId(User);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(
                "Details",
                "Posts",
                new { slug = newComment.Post.Slug },
                "commentSection"
            ); // in View, there is an id for every comment <h4> so after editing, View will stop right there
            // return RedirectToAction("Details", "Posts", new {slug = newComment.Post.Slug}, $"commentNumber_{comment.Id}"); // in View, there is an id for every comment <h4> so after editing, View will stop right there
        }

        return RedirectToAction("Details", "Posts", new { slug = postSlug }, "commentSection");
    }

    [Authorize]
    // GET: Comments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var comment = await _context.Comments
            .Include(c => c.BlogUser)
            .Include(c => c.Moderator)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (comment == null) return NotFound();

        if (
            !User.IsInRole(BlogRole.Moderator.ToString())
            && comment.BlogUser.Id != _userManager.GetUserId(User)
        )
            throw new InvalidOperationException(
                "You are not authoried to delete other people comments!"
            );

        return View(comment);
    }

    [Authorize]
    // POST: Comments/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string slug)
    {
        var comment = await _context.Comments
            .Include(c => c.BlogUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return NotFound();

        if (
            !User.IsInRole(BlogRole.Moderator.ToString())
            && comment.BlogUser.Id != _userManager.GetUserId(User)
        )
            throw new InvalidOperationException(
                "You are not authorized to delete other people comments!"
            );

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Posts", new { slug }, "commentSection");
    }

    private bool CommentExists(int id)
    {
        return _context.Comments.Any(e => e.Id == id);
    }

    //
    // [CanBeNull]
    // private string GetModelStateErrorMessage()
    // {
    //     return ModelState.Where(
    //         x => x.Value.Errors.Count > 0
    //     ).Select(
    //         x => new { x.Key, x.Value.Errors }
    //     ).FirstOrDefault().Errors.First().ErrorMessage;
    // }

    private string GetFirstModelStateErrorMessage()
    {
        return ViewData.ModelState.Values
            .SelectMany(x => x.Errors)
            .FirstOrDefault(err => !string.IsNullOrWhiteSpace(err.ErrorMessage))
            ?.ErrorMessage;
    }
}