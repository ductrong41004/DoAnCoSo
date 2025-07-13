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
        public async Task<IActionResult> Register(string FullName, string Email, string Password, string Phone)
        {
            if (_context.Users.Any(u => u.Email == Email))
            {
                TempData["RegisterError"] = "Email đã tồn tại!";
                return RedirectToAction("Index", "Tours");
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

            return RedirectToAction("Index", "Tours");
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == Email);
            if (user == null)
            {
                TempData["LoginError"] = "Email không tồn tại!";
                return RedirectToAction("Index", "Tours");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
            {
                TempData["LoginError"] = "Mật khẩu không đúng!";
                return RedirectToAction("Index", "Tours");
            }

            // Lưu session đăng nhập
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName ?? "");

            TempData["LoginSuccess"] = "Đăng nhập thành công!";

            return RedirectToAction("Index", "Tours");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Tours");
        }
    }
}
