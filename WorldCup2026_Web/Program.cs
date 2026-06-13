using Microsoft.EntityFrameworkCore;
using WorldCup2026_Web.Data;
using WorldCup2026_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// SQL Server Baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Groq AI Servis Kaydý (YENÝ)
builder.Services.AddHttpClient<GroqService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Veri Tabaný Otopilot Yükleme Motorunu Tetikliyoruz
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        app.InitializeData();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veri tabaný seed edilirken bir hata oluþtu.");
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();