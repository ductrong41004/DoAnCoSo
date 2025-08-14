using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TourDuLich.Data;
using TourDuLich.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace TourDuLich.Controllers
{
    public class AccountController : Controller
    {
        private readonly TourDuLichContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(TourDuLichContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> Register(string FullName, string Email, string Password, string Phone, string ReturnUrl)
        {
            if (_context.Users.Any(u => u.Email == Email))
            {
                TempData["RegisterError"] = "Email đã tồn tại!";
                TempData["ShowLoginModal"] = "true";
                TempData["ActiveTab"] = "register";
                return RedirectToReturnUrl(ReturnUrl);
            }

            var user = new User
            {
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            TempData["ShowLoginModal"] = "true";
            TempData["ActiveTab"] = "login";

            return RedirectToReturnUrl(ReturnUrl);
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password, string ReturnUrl)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == Email);
            if (user == null)
            {
                TempData["LoginError"] = "Email không tồn tại!";
                return RedirectToReturnUrl(ReturnUrl);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
            {
                TempData["LoginError"] = "Mật khẩu không đúng!";
                return RedirectToReturnUrl(ReturnUrl);
            }

            // Lưu session đăng nhập
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName ?? "");
            HttpContext.Session.SetString("UserType", user.Type.ToString());        // ✅ Cho hiển thị trang chủ
            HttpContext.Session.SetInt32("UserTypeInt", (int)user.Type);            // ✅ Cho kiểm tra quyền admin

            // ✅ THÊM DEBUG
            System.Diagnostics.Debug.WriteLine($"User Type: {user.Type}");
            System.Diagnostics.Debug.WriteLine($"User Type Int: {(int)user.Type}");
            System.Diagnostics.Debug.WriteLine($"Session UserTypeInt: {HttpContext.Session.GetInt32("UserTypeInt")}");

            TempData["LoginSuccess"] = "Đăng nhập thành công!";

            return RedirectToReturnUrl(ReturnUrl);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Tours");
        }

        // Helper method để xử lý ReturnUrl
        private IActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Tours");
        }
    }
}