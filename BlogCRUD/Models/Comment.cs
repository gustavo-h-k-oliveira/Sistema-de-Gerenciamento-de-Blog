using System.ComponentModel.DataAnnotations;

namespace BlogCRUD.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        [StringLength(500)]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public required virtual Post Post { get; set; }
        public required virtual ApplicationUser User { get; set; }
    }
}