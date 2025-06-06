using System.ComponentModel.DataAnnotations;

namespace BlogCRUD.Models
{
    public class Tag
    {
        public int Id { get; set; }
    
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
