using Microsoft.EntityFrameworkCore;
using BlogCRUD.Models;

namespace BlogCRUD.Data
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options)
            : base(options) {}

        public DbSet<Post> Posts {get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}