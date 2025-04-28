using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlogCRUD.Models;
using BlogCRUD.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BlogCRUD.Data;

namespace BlogCRUD.Controllers
{
    public class AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        BlogContext context) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly BlogContext _context = context;

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verifica se o email está banido
            if (_context.BannedEmails.Any(be => be.Email == model.Email))
            {
                ModelState.AddModelError(string.Empty, "This email is banned from registering.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Reader"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Reader"));
                }
                await _userManager.AddToRoleAsync(user, "Reader");

                if (!await _roleManager.RoleExistsAsync("Editor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Editor"));
                }
                await _userManager.AddToRoleAsync(user, "Editor");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl ?? string.Empty);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User)!;
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return Forbid(); // Ou outra ação apropriada
            }

            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Adiciona o email à tabela de emails banidos
            var bannedEmail = new BannedEmail { Email = user.Email ?? string.Empty };
            _context.BannedEmails.Add(bannedEmail);

            // Remove o usuário da plataforma
            await _userManager.DeleteAsync(user);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
