using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ArabaKiralamaSistemi.Areas.Identity.Data; // O kırmızı hata çıkmasın diye bunu ekledim

namespace ArabaKiralamaSistemi.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly ArabaKiralamaSistemiContext _context;
        private readonly UserManager<ArabaKiralamaSistemiUser> _userManager;

        public RentalsController(ArabaKiralamaSistemiContext context, UserManager<ArabaKiralamaSistemiUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Kiralama Sayfasını Göster
        public async Task<IActionResult> Rent(int carId)
        {
            var car = await _context.Car.FindAsync(carId);

            if (car == null || !car.IsAvailable)
            {
                return NotFound("Bu araç şu an müsait değil.");
            }

            var rental = new Rental
            {
                CarId = carId,
                Car = car,
                RentDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(1)
            };

            return View(rental);
        }

        // 2. Kiralama İşlemini Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(Rental rental)
        {
            var car = await _context.Car.FindAsync(rental.CarId);
            if (car == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            rental.UserId = user.Id;

            if (rental.ReturnDate <= rental.RentDate)
            {
                ModelState.AddModelError("", "İade tarihi alış tarihinden sonra olmalıdır!");
                rental.Car = car;
                return View(rental);
            }

            var gunSayisi = (rental.ReturnDate - rental.RentDate).Days;
            rental.TotalPrice = gunSayisi * car.DailyPrice;

            _context.Add(rental);

            // Arabayı "Müsait Değil" yap
            car.IsAvailable = false;
            _context.Update(car);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Cars");
        }

        // 3. Admin Paneli: Tüm Kiralamaları Listele
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rental
                .Include(r => r.Car)
                .ToListAsync();

            return View(rentals);
        }

        // 4. Admin Paneli: Arabayı Teslim Al (İade)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ReturnCar(int id)
        {
            var rental = await _context.Rental.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == id);

            if (rental != null)
            {
                rental.Car.IsAvailable = true; // Arabayı yeşile çevir
                _context.Update(rental.Car);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    } // Class burada bitiyor
} // Namespace burada bitiyor