using System;
using System.ComponentModel.DataAnnotations;
using TheBlog.Enums;

namespace TheBlog.Models;

public sealed class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string BlogUserId { get; set; }
    public string ModeratorId { get; set; }

    [Required]
    [StringLength(
        500,
        ErrorMessage = "The {0} must be between {2} and {1} characters long.",
        MinimumLength = 2
    )]
    [Display(Name = "Comment")]
    public string Body { get; set; }

    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Moderated { get; set; }
    public DateTime? Deleted { get; set; }

    [StringLength(
        500,
        ErrorMessage = "The {0} must be between {2} and {1} characters long.",
        MinimumLength = 2
    )]
    public string ModeratedBody { get; set; }

    public ModerationType ModerationType { get; set; }

    // Navigation properties -> they will get the whole object they refer to
    public Post Post { get; set; }
    public BlogUser BlogUser { get; set; }
    public BlogUser Moderator { get; set; }
}