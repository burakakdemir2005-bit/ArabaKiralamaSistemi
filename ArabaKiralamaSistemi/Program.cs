using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Areas.Identity.Data;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. YENİ POSTGRESQL BAĞLANTISI (FINAL) ---
// Render'dan aldığın adresi tam olarak buraya yerleştirdim.
var connectionString = "Host=dpg-d573o76uk2gs73cqa5vg-a.frankfurt-postgres.render.com;Port=5432;Database=araba_veritabani_final;Username=araba_veritabani_final_user;Password=cSBEpguDT78WbtOlYzlrODYjUzF7P2EM;SSL Mode=Require;Trust Server Certificate=true";

builder.Services.AddDbContext<ArabaKiralamaSistemiContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<AuthContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. IDENTITY AYARLARI ---
builder.Services.AddDefaultIdentity<ArabaKiralamaSistemiUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// --- 3. OTOMATİK TABLO VE ADMİN KURULUMU (KRİTİK) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
        var authContext = services.GetRequiredService<AuthContext>();

        // Yeni veritabanını algılar ve tüm tabloları sıfırdan oluşturur
        context.Database.EnsureCreated();
        authContext.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ArabaKiralamaSistemiUser>>();

        if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
        }

        string email = "burakkakdemirr453@gmail.com";
        var user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

        if (user == null)
        {
            user = new ArabaKiralamaSistemiUser { UserName = email, Email = email, EmailConfirmed = true };
            userManager.CreateAsync(user, "Burak123!").GetAwaiter().GetResult();
        }

        if (user != null && !userManager.IsInRoleAsync(user, "Admin").GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
        }
    }
    catch (Exception ex) { /* Hata loglanabilir */ }
}

// --- 4. MIDDLEWARE ---
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();