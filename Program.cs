using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TourDuLich.Data;
using TourDuLich.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TourDuLichContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký PasswordHasher để mã hóa mật khẩu User
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Đăng ký Session
builder.Services.AddSession();

// Đăng ký IHttpContextAccessor để lấy session trong View (Layout)
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

// Thêm middleware Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tours}/{action=Index}/{id?}");

app.Run();
