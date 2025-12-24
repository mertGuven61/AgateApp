using AgateApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritaban� Ba�lant�s�
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AgateDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity (Kimlik) Servisinin Eklenmesi (MANUEL EKLEME)
// Admin ve Staff rolleri olaca�� i�in .AddRoles<IdentityRole>() ekliyoruz.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // �ifre kurallar�n� basitle�tiriyoruz (Test kolayl��� i�in)
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3; // En az 3 karakter �ifre
})
    .AddRoles<IdentityRole>() // Rolleri aktif et
    .AddEntityFrameworkStores<AgateDbContext>();

builder.Services.AddControllersWithViews();

// Groq API Servisi (En hızlı ve güvenilir)
builder.Services.AddHttpClient<AgateApp.Services.GroqService>();
builder.Services.AddScoped<AgateApp.Services.GroqService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Kimlik Do�rulama S�ras� (�nemli!)
app.UseAuthentication(); // �nce: Kimsin?
app.UseAuthorization();  // Sonra: Yetkin var m�?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}"); // A��l�� sayfas� Dashboard

app.MapRazorPages(); // Identity sayfalar� (Login/Register) i�in gerekli

// --- BA�LANGI� VER�LER�N� (Admin/Staff) Y�KLEME ---
// Bu k�s�m veritaban�nda Admin kullan�c�s� yoksa olu�turur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Birazdan olu�turaca��m�z RoleInitializer s�n�f�n� �a��r�yoruz
        await AgateApp.Data.RoleInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// -------------------------------------

app.Run();