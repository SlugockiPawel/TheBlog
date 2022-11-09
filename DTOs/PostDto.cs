using System;
using System.ComponentModel.DataAnnotations;

namespace TheBlog.DTOs;

public class PostDto
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Abstract { get; set; }
    public byte[] ImageData { get; set; }
    public string ContentType { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime Created { get; set; }
}