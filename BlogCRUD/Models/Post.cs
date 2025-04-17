using System;
using System.ComponentModel.DataAnnotations;

namespace BlogCRUD.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(250)]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DatePublished { get; set; } = DateTime.Now;
    }
}