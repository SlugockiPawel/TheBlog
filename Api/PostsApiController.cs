using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TheBlog.Data;
using TheBlog.DTOs;
using TheBlog.Enums;

namespace TheBlog.Api;

[Route("api/posts/")]
[ApiController]
public class PostsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{count:int}")]
    public ActionResult<IEnumerable<PostDto>> GetPosts(int count)
    {
        var posts = _context.Posts
            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
            .OrderByDescending(p => p.Created)
            .Take(count)
            .Select(
                p =>
                    new PostDto
                    {
                        Title = p.Title,
                        Abstract = p.Abstract,
                        ImageData = p.ImageData,
                        ContentType = p.ContentType,
                        Created = DateTime.SpecifyKind(p.Created, DateTimeKind.Utc)
                    }
            )
            .ToList();

        if (posts.Count > 0)
        {
            return Ok(posts);
        }

        return NotFound();
    }
}
