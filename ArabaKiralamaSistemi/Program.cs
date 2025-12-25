using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ArabaKiralamaSistemi.Data;
using Microsoft.AspNetCore.Identity;
using ArabaKiralamaSistemi.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ArabaKiralamaSistemiContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ArabaKiralamaSistemiContext") ?? throw new InvalidOperationException("Connection string 'ArabaKiralamaSistemiContext' not found.")));
builder.Services.AddDbContext<AuthContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthContextConnection")));
builder.Services.AddDefaultIdentity<ArabaKiralamaSistemiUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<AuthContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
// --- OTOMATİK ADMİN OLUŞTURMA KODU (BAŞLANGIÇ) ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ArabaKiralamaSistemiUser>>();

    // 1. "Admin" rolü yoksa oluştur
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
    }

    // 2. Senin mail adresini bul ve Admin yap
    // BURAYA KENDİ E-POSTANI YAZ 👇
    var adminUser = await userManager.FindByEmailAsync("burakkakdemirr453@gmail.com");

    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// --- RENDER İÇİN KESİN ÇÖZÜM: TABLOLARI ZORLA OLUŞTUR ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Arabalar Veritabanını Zorla Kur
        var context = services.GetRequiredService<ArabaKiralamaSistemi.Data.ArabaKiralamaSistemiContext>();
        context.Database.EnsureCreated(); // Migrate yerine bunu kullanıyoruz

        // 2. Kullanıcılar (Auth) Veritabanını Zorla Kur
        var authContext = services.GetRequiredService<AuthContext>();
        authContext.Database.EnsureCreated(); // Tabloları yoktan var et!
    }
    catch (Exception ex)
    {
        // Hata olursa en azından loglarda görelim
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken hata çıktı!");
    }
}
app.Run();
