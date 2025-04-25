using BlogCRUD.Data;
using BlogCRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogCRUD.Controllers
{
    [Authorize]
    public class CommentController(BlogContext context, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly BlogContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comments can't be empty.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                Post = post,
                User = user
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}