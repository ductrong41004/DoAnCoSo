using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TourDuLich.Data;
using TourDuLich.Models;

var builder = WebApplication.CreateBuilder(args);

// Them cac dich vu vao container
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TourDuLichContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dang ky PasswordHasher de ma hoa mat khau User
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Session can cache phan tan in-memory
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Dang ky IHttpContextAccessor de lay session trong View Layout
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Them middleware Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tours}/{action=Index}/{id?}");

// Log ra DB hien tai app dang dung
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourDuLichContext>();
    var conn = db.Database.GetDbConnection();
    Console.WriteLine($"DB Source={conn.DataSource}; DB Name={conn.Database}");
}

app.Run();
