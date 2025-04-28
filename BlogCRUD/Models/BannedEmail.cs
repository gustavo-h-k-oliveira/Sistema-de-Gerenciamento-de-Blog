using System.ComponentModel.DataAnnotations;

namespace BlogCRUD.Models
{
    public class BannedEmail
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    }
}