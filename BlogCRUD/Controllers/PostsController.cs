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
        public async Task<IActionResult> Index(string searchTerm, int? categoryId)
        {
            // Passa os valores para a View
            ViewData["SearchTerm"] = searchTerm;
            ViewData["CategoryId"] = categoryId;

            // Preenche o dropdown de categorias
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // Consulta inicial
            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsQueryable();

            // Filtra por termo de busca
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm));
            }

            // Filtra por categoria
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Retorna os resultados
            var posts = await query.ToListAsync();
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
            var vm = new BlogCRUD.ViewModels.PostViewModel
            {
                AllTags = _context.Tags.ToList()
            };
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", 1);
            return View(vm);
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogCRUD.ViewModels.PostViewModel vm)
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

                if (vm.CategoryId == 0 || !_context.Categories.Any(c => c.Id == vm.CategoryId))
                {
                    vm.CategoryId = generalCategory.Id;
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var post = new Post
                {
                    Title = vm.Title,
                    Content = vm.Content,
                    CategoryId = vm.CategoryId,
                    UserId = user.Id,
                    DatePublished = DateTime.Now,
                    PostTags = vm.SelectedTagIds.Select(tagId => new PostTag { TagId = tagId }).ToList()
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.AllTags = _context.Tags.ToList();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", vm.CategoryId);
            return View(vm);
        }

        // GET: Posts/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (post.UserId != user.Id && !isAdmin) return Forbid();

            var vm = new BlogCRUD.ViewModels.PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CategoryId = post.CategoryId,
                SelectedTagIds = post.PostTags.Select(pt => pt.TagId).ToList(),
                AllTags = _context.Tags.ToList()
            };

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(vm);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogCRUD.ViewModels.PostViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", vm.CategoryId);

            if (ModelState.IsValid)
            {
                var post = await _context.Posts
                    .Include(p => p.PostTags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null) return NotFound();

                post.Title = vm.Title;
                post.Content = vm.Content;
                post.CategoryId = vm.CategoryId;

                // Atualiza tags
                post.PostTags.Clear();
                foreach (var tagId in vm.SelectedTagIds)
                {
                    post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });
                }

                _context.Update(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.AllTags = _context.Tags.ToList();
            return View(vm);
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
