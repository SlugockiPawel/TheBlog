using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TheBlog.Models;

public sealed class Category
{
    public int Id { get; set; }
    public string BlogUserId { get; set; }

    [Required]
    [StringLength(
        100,
        ErrorMessage = "The {0} must be at least {2} and at most {1} characters.",
        MinimumLength = 2
    )]
    public string Name { get; set; }

    [Required]
    [StringLength(
        500,
        ErrorMessage = "The {0} must be at least {2} and at most {1} characters.",
        MinimumLength = 5
    )]
    public string Description { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Created Date")]
    public DateTime Created { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Updated Date")]
    public DateTime? Updated { get; set; }

    [Display(Name = "Blog Image")] public byte[] ImageData { get; set; }

    [Display(Name = "Image Type")] public string ContentType { get; set; }

    [NotMapped] public IFormFile Image { get; set; }

    // Navigation Properties

    [DisplayName("Author")] public BlogUser BlogUser { get; set; } // Author is a parent for Blog

    public ICollection<Post> Posts { get; set; } = new HashSet<Post>(); // Blog is a parent for Post
}