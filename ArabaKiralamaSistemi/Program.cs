using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Data;
using ArabaKiralamaSistemi.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. POSTGRESQL BAĞLANTISI ---
// Render PostgreSQL bilgilerini tek tek parçalayarak sisteme tanıtıyoruz
var connectionString = "Host=dpg-d56qhi0gjchc73973arg-a.frankfurt-postgres.render.com;Port=5432;Database=araba_veritabani;Username=araba_veritabani_user;Password=NEUMjPs8i14GHthKX6Li9SpCSqgRXIK5;SSL Mode=Require;Trust Server Certificate=true";
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
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
    context.Database.EnsureCreated();
}

// --- 3. OTOMATİK VERİTABANI VE ADMİN KURULUMU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ArabaKiralamaSistemiContext>();
        context.Database.EnsureCreated();
        var authContext = services.GetRequiredService<AuthContext>();
        authContext.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var userManager = services.GetRequiredService<UserManager<ArabaKiralamaSistemiUser>>();
        string email = "burakkakdemirr453@gmail.com";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ArabaKiralamaSistemiUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "Burak123!");
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı kurulum hatası!");
    }
}

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
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();