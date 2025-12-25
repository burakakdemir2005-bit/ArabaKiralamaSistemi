using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantıları
var connectionString = builder.Configuration.GetConnectionString("ArabaKiralamaSistemiContext") ?? throw new InvalidOperationException("Connection string 'ArabaKiralamaSistemiContext' not found.");

builder.Services.AddDbContext<ArabaKiralamaSistemiContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContext<AuthContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthContextConnection") ?? connectionString));

// 2. Identity Ayarları
builder.Services.AddDefaultIdentity<ArabaKiralamaSistemiUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Rolleri aktif et
    .AddEntityFrameworkStores<AuthContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// --- RENDER İÇİN OTOMATİK ADMİN OLUŞTURMA (HİLE KODU) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // A. Tabloları Oluştur
        var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
        context.Database.EnsureCreated();
        var authContext = services.GetRequiredService<AuthContext>();
        authContext.Database.EnsureCreated();

        // B. Admin Rolü ve Yetkisi Ver
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ArabaKiralamaSistemiUser>>();

        // "Admin" rolü yoksa oluştur
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // --- DÜZELTİLEN KISIM BURASI (2 'r' ile) ---
        string benimMailim = "burakkakdemirr453@gmail.com";
        // ---------------------------------------------

        var user = await userManager.FindByEmailAsync(benimMailim);

        if (user != null)
        {
            // Kullanıcı bulunduysa ona Admin rolü ver
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı veya Admin yetkisi oluşturulurken hata çıktı.");
    }
}
// -------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();