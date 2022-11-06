using System.ComponentModel.DataAnnotations;

namespace TheBlog.Models;

public sealed class Tag
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string BlogUserId { get; set; }

    [Required]
    [StringLength(25, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
    public string Text { get; set; }

    // Navigation properties -> they will get the whole object they refer to
    public Post Post { get; set; }
    public BlogUser BlogUser { get; set; }
}