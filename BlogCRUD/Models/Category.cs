using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogCRUD.Models
{
    [Table("Categories")]
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
