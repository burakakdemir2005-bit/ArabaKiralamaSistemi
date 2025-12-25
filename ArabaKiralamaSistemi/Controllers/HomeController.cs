using ArabaKiralamaSistemi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ArabaKiralamaSistemi.Areas.Identity.Data;

namespace ArabaKiralamaSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ArabaKiralamaSistemiUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ArabaKiralamaSistemiUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- SÝHÝRLÝ ADMÝN YAPMA LÝNKÝ ---
        [Authorize]
        public async Task<IActionResult> MakeMeAdmin()
        {
            // 1. Önce Admin rolü var mý kontrol et, yoksa oluþtur
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Þu anki giriþ yapmýþ kullanýcýyý bul
            var user = await _userManager.GetUserAsync(User);

            // 3. Kullanýcýya Admin rolünü ver
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Content($"TEBRÝKLER {user.UserName}! Artýk Admin yetkisine sahipsiniz. Þimdi çýkýþ yapýp tekrar girin.");
            }

            return Content("Hata: Kullanýcý bulunamadý.");
        }
        // ----------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}