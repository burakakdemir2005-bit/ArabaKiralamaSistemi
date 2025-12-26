using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Areas.Identity.Data;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. POSTGRESQL BAĞLANTISI (YENİ DB: fz22) ---
// Gönderdiğin şifreyi (3iZVq...) buraya tam olarak yerleştirdim.
var connectionString = "Host=dpg-d572u9f5r7bs73ftatmg-a.frankfurt-postgres.render.com;Port=5432;Database=araba_veritabani_fz22;Username=araba_veritabani_user;Password=3iZVq70CVzX76IIBwOQxegnblafLCvzv;SSL Mode=Require;Trust Server Certificate=true";

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

// --- 3. OTOMATİK VERİTABANI, TABLO VE ADMİN KURULUMU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
        var authContext = services.GetRequiredService<AuthContext>();

        // Bu satırlar 'AspNetUsers' hatasını kesin olarak çözer (Tabloları sıfırdan yaratır)
        context.Database.EnsureCreated();
        authContext.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ArabaKiralamaSistemiUser>>();

        // Admin rolünü kontrol et ve oluştur
        if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
        }

        // Admin kullanıcısını kontrol et ve oluştur
        string email = "burakkakdemirr453@gmail.com";
        var user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

        if (user == null)
        {
            user = new ArabaKiralamaSistemiUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            userManager.CreateAsync(user, "Burak123!").GetAwaiter().GetResult();
        }

        // Kullanıcıya Admin yetkisi ver
        if (user != null && !userManager.IsInRoleAsync(user, "Admin").GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı kurulumu sırasında bir hata oluştu.");
    }
}

// --- 4. MIDDLEWARE AYARLARI ---
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