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
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DatePublished { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "The Category field is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}