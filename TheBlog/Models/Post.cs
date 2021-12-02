using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TheBlog.Enums;

namespace TheBlog.Models
{
    using System.ComponentModel;

    public class Post
    {
        public int Id { get; set; }

        [DisplayName("Category Name")]
        public int CategoryId { get; set; }
        public string BlogUserId { get; set; }

        [Required]
        [StringLength(75, ErrorMessage = "The {0} has to be between {2} and {1} characters long", MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} has to be between {2} and {1} characters long", MinimumLength = 2)]
        public string Abstract { get; set; }

        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

        public ReadyStatus ReadyStatus { get; set; }

        public string Slug { get; set; }

        public byte[] ImageData { get; set; }
        public string ContentType { get; set; }

        [NotMapped]
        public IFormFile Image { get; set; }


        //Navigation Properties
        public virtual Category Category { get; set; } // Category is a parent for Post
        public virtual BlogUser BlogUser { get; set; } // Author is a parent for Post

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>(); // Post is a parent for Tag
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>(); // Post is a parent for Comment

    }
}
