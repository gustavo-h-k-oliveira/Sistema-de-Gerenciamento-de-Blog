using BlogCRUD.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogCRUD.Models;

[Route("category")]
public class CategoriesController : Controller
{
    private readonly BlogContext _context;

    public CategoriesController(BlogContext context)
    {
        _context = context;
    }

    // GET: Categories
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.ToListAsync());
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Id == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost("delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}