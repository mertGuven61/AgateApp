using AgateApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AgateDbContext>(options =>
	options.UseSqlServer(connectionString));

// 2. Identity (Kimlik) Servisinin Eklenmesi (MANUEL EKLEME)
// Admin ve Staff rolleri olacaðý için .AddRoles<IdentityRole>() ekliyoruz.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
	// Þifre kurallarýný basitleþtiriyoruz (Test kolaylýðý için)
	options.SignIn.RequireConfirmedAccount = false;
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 3; // En az 3 karakter þifre
})
	.AddRoles<IdentityRole>() // Rolleri aktif et
	.AddEntityFrameworkStores<AgateDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
	.AddViewLocalization()
	.AddDataAnnotationsLocalization();


var supportedCultures = new[] { "en-US", "tr-TR" };
var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture("tr-TR") // Varsayýlan dil
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures);




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

app.UseRequestLocalization(localizationOptions);
// 3. Kimlik Doðrulama Sýrasý (Önemli!)
app.UseAuthentication(); // Önce: Kimsin?
app.UseAuthorization();  // Sonra: Yetkin var mý?

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Dashboard}/{action=Index}/{id?}"); // Açýlýþ sayfasý Dashboard

app.MapRazorPages(); // Identity sayfalarý (Login/Register) için gerekli

// --- BAÞLANGIÇ VERÝLERÝNÝ (Admin/Staff) YÜKLEME ---
// Bu kýsým veritabanýnda Admin kullanýcýsý yoksa oluþturur
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		// Birazdan oluþturacaðýmýz RoleInitializer sýnýfýný çaðýrýyoruz
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