using System.ComponentModel.DataAnnotations;
using BlogCRUD.Models;

namespace BlogCRUD.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }
    
        [Required]
        public string Title { get; set; } = string.Empty;
    
        [Required]
        public string Content { get; set; } = string.Empty;
    
        [Required]
        public int CategoryId { get; set; }
    
        public List<int> SelectedTagIds { get; set; } = new();
        public List<Tag> AllTags { get; set; } = new();
        public DateTime DatePublished { get; set; }
    }   
}