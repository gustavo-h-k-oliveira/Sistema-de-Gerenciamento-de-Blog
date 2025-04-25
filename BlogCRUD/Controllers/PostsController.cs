using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogCRUD.Data;
using BlogCRUD.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BlogCRUD.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class PostsController : Controller

    {
        private readonly BlogContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public PostsController(BlogContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Category) 
                .Include(p => p.User)     
                .Include(p => p.Comments)
                .ToListAsync();

            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", 1);
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (ModelState.IsValid)
            {
                var generalCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "General");
                if (generalCategory == null)
                {
                    generalCategory = new Category { Name = "General" };
                    _context.Categories.Add(generalCategory);
                    await _context.SaveChangesAsync();
                }

                if (post.CategoryId == 0 || !_context.Categories.Any(c => c.Id == post.CategoryId))
                {
                    post.CategoryId = generalCategory.Id;
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                post.UserId = user.Id;
                post.DatePublished = DateTime.Now;

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (post.UserId != user.Id && !isAdmin) return Forbid();
            
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,DatePublished,CategoryId")] Post post)
        {
            if (id != post.Id) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);

            if (ModelState.IsValid)
            {
                if (!_context.Categories.Any(c => c.Id == post.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", "The selected category is invalid.");
                    return View(post);
                }

                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (post.UserId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
