using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantılarını Ayarla (SQLite)
var connectionString = builder.Configuration.GetConnectionString("ArabaKiralamaSistemiContext") ?? throw new InvalidOperationException("Connection string 'ArabaKiralamaSistemiContext' not found.");

builder.Services.AddDbContext<ArabaKiralamaSistemiContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContext<AuthContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthContextConnection") ?? connectionString));

// 2. Identity (Kullanıcı Giriş) Ayarları
builder.Services.AddDefaultIdentity<ArabaKiralamaSistemiUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Rolleri aktif et
    .AddEntityFrameworkStores<AuthContext>();

// 3. MVC Controller ve View'leri ekle
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// --- RENDER İÇİN KESİN ÇÖZÜM: TABLOLARI EN BAŞTA OLUŞTUR ---
// Bu blok uygulama çalışmaya başlamadan HEMEN ÖNCE veritabanını kurar.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Önce Arabalar Veritabanı
        var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
        context.Database.EnsureCreated();

        // Sonra Kullanıcı (Auth) Veritabanı
        var authContext = services.GetRequiredService<AuthContext>();
        authContext.Database.EnsureCreated();

        // (İsteğe Bağlı) Otomatik Admin Rolü Ekleme
        // Eğer veritabanı boşsa hata vermesin diye try-catch içinde kalsın
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu.");
    }
}
// -------------------------------------------------------

// 4. Middleware Ayarları (Standart)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Giriş yapma (Önemli: Authorization'dan önce olmalı)
app.UseAuthorization();  // Yetki kontrolü

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();