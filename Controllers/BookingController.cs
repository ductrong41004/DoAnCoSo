using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourDuLich.Data;
using TourDuLich.Models;
using System.ComponentModel.DataAnnotations;

namespace TourDuLich.Controllers
{
    public class BookingController : Controller
    {
        private readonly TourDuLichContext _context;

        public BookingController(TourDuLichContext context)
        {
            _context = context;
        }

        // GET: /Booking/Checkout/5 - Trang thanh toan
        [HttpGet]
        public async Task<IActionResult> Checkout(int id, string? departureDate = null)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["LoginRequired"] = "Vui lòng đăng nhập để đặt tour!";
                return RedirectToAction("Detail", "Tours", new { id = id });
            }

            // Lay thong tin tour voi bookings de tinh so cho con lai
            var tour = await _context.Tours
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            // Lay thong tin user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToAction("Detail", "Tours", new { id = id });
            }

            // Xu ly ngay khoi hanh duoc chon
            DateTime selectedDepartureDate;
            if (!string.IsNullOrEmpty(departureDate) && DateTime.TryParse(departureDate, out selectedDepartureDate))
            {
                // Su dung ngay duoc chon
            }
            else
            {
                // Su dung ngay khoi hanh gan nhat
                selectedDepartureDate = tour.GetNextDepartureDate() ?? DateTime.MinValue;
            }

            // Tao ViewModel
            var viewModel = new BookingCheckoutViewModel
            {
                Tour = tour,
                User = user,
                SelectedDepartureDate = selectedDepartureDate,
                AdultCount = 1,
                ChildCount = 0,
                InfantCount = 0,
                BabyCount = 0,
                SingleRoomSupplement = false,
                SingleRoomCount = 0
            };

            return View(viewModel);
        }

        // POST: /Booking/Process - Xu ly dat tour
        [HttpPost]
        public async Task<IActionResult> Process(BookingCheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Tours");
            }

            if (!ModelState.IsValid)
            {
                // Debug: Log validation errors
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                // Reload tour data if validation fails
                model.Tour = await _context.Tours.FirstOrDefaultAsync(t => t.TourId == model.TourId);
                model.User = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                // Set default values if missing
                if (model.Tour != null && model.User != null)
                {
                    model.FullName = model.FullName ?? model.User.FullName ?? "";
                    model.Email = model.Email ?? model.User.Email ?? "";
                    model.Phone = model.Phone ?? model.User.Phone ?? "";
                }

                return View("Checkout", model);
            }

            // Tính tổng giá
            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.TourId == model.TourId);
            if (tour == null)
            {
                return NotFound();
            }

            // ✅ KIỂM TRA SỐ CHỖ CÒN LẠI CHO NGÀY CỤ THỂ (chỉ tính booking đã confirmed)
            var availableSeats = tour.GetInitialSeatsForDate(model.SelectedDepartureDate);
            var totalPassengers = model.AdultCount + model.ChildCount + model.InfantCount + model.BabyCount;

            // Tính số chỗ đã được đặt (CHỈ CONFIRMED) cho ngày này
            var bookedSeats = await _context.Bookings
                .Where(b => b.TourId == tour.TourId &&
                           b.DepartureDate.Date == model.SelectedDepartureDate.Date &&
                           b.Status == "Confirmed") // ✅ CHỈ KIỂM TRA CONFIRMED
                .SumAsync(b => b.AdultCount + b.ChildCount + b.InfantCount + b.BabyCount);

            var actualAvailableSeats = availableSeats - bookedSeats;

            if (actualAvailableSeats < totalPassengers)
            {
                ModelState.AddModelError("", $"Không đủ chỗ! Ngày {model.SelectedDepartureDate:dd/MM/yyyy} chỉ còn {actualAvailableSeats} chỗ trống, bạn đang đặt {totalPassengers} chỗ.");

                // Reload data và return view
                model.Tour = tour;
                model.User = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                return View("Checkout", model);
            }

            decimal totalPrice = CalculateTotalPrice(tour.Price, model);

            // Tạo booking với thông tin đầy đủ
            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var booking = new Booking
            {
                UserId = userId.Value,
                TourId = model.TourId,
                BookingDate = vietnamNow,
                DepartureDate = model.SelectedDepartureDate != DateTime.MinValue ? model.SelectedDepartureDate : tour.DepartureDate,
                TotalPrice = totalPrice,
                Status = "Pending", // Chờ xác nhận
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Pending", // Chờ thanh toán

                // Thông tin khách hàng
                CustomerName = model.FullName,
                CustomerEmail = model.Email,
                CustomerPhone = model.Phone,
                CustomerAddress = model.Address,
                Notes = model.Notes,

                // Thông tin hành khách
                AdultCount = model.AdultCount,
                ChildCount = model.ChildCount,
                InfantCount = model.InfantCount,
                BabyCount = model.BabyCount,
                SingleRoomSupplement = model.SingleRoomSupplement,
                SingleRoomCount = model.SingleRoomCount
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Chuyển hướng dựa trên phương thức thanh toán
            if (model.PaymentMethod == "BankTransfer")
            {
                // Chuyển đến trang thanh toán chuyển khoản
                return RedirectToAction("BankTransfer", new { id = booking.BookingId });
            }
            else if (model.PaymentMethod == "Crypto")
            {
                // TODO: Chuyển đến trang thanh toán crypto (sẽ làm sau)
                TempData["BookingSuccess"] = $"Đặt tour thành công! Mã đơn hàng: #{booking.BookingId}. " +
                                           "Vui lòng chuyển tiền điện tử theo địa chỉ đã cung cấp.";
                return RedirectToAction("Index", "Tours");
            }

            return RedirectToAction("Index", "Tours");
        }

        // Helper method to calculate total price
        private decimal CalculateTotalPrice(decimal basePrice, BookingCheckoutViewModel model)
        {
            decimal total = 0;

            // Người lớn (100%)
            total += basePrice * model.AdultCount;

            // Trẻ em 5-11 tuổi (90%)
            decimal childPrice = Math.Round(basePrice * 0.9m / 100000) * 100000; // Làm tròn đến 100k
            total += childPrice * model.ChildCount;

            // Trẻ nhỏ 2-5 tuổi (47%)
            decimal infantPrice = Math.Round(basePrice * 0.47m / 100000) * 100000; // Làm tròn đến 100k
            total += infantPrice * model.InfantCount;

            // Sơ sinh < 2 tuổi (7%)
            decimal babyPrice = Math.Round(basePrice * 0.07m / 100000) * 100000; // Làm tròn đến 100k
            total += babyPrice * model.BabyCount;

            // Phụ thu phòng đơn
            if (model.SingleRoomSupplement && model.SingleRoomCount > 0)
            {
                total += 1300000 * model.SingleRoomCount; // 1.3 triệu/phòng
            }

            return total;
        }

        // GET: /Booking/BankTransfer/5
        [HttpGet]
        public async Task<IActionResult> BankTransfer(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Tours");
            }

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            var viewModel = new PaymentViewModel
            {
                BookingId = booking.BookingId,
                Tour = booking.Tour!,
                TotalAmount = booking.TotalPrice,
                CustomerName = booking.CustomerName ?? booking.User?.FullName ?? "",
                CustomerEmail = booking.CustomerEmail ?? booking.User?.Email ?? "",
                CustomerPhone = booking.CustomerPhone ?? booking.User?.Phone ?? "",
                DepartureDate = booking.DepartureDate,
                TotalPassengers = booking.AdultCount + booking.ChildCount + booking.InfantCount + booking.BabyCount
            };

            return View(viewModel);
        }

        // GET: /Booking/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["LoginRequired"] = "Vui lòng đăng nhập để xem danh sách tour đã đặt!";
                return RedirectToAction("Index", "Tours");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: /Booking/GetBookingDetails
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails(int bookingId)
        {
            try
            {
                // Kiểm tra đăng nhập
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                // Lấy booking của user hiện tại
                var booking = await _context.Bookings
                    .Include(b => b.Tour)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem!" });
                }

                var details = new
                {
                    success = true,
                    data = new
                    {
                        bookingId = booking.BookingId,
                        bookingDate = booking.BookingDate,
                        customerName = booking.CustomerName,
                        customerEmail = booking.CustomerEmail,
                        customerPhone = booking.CustomerPhone,
                        customerAddress = booking.CustomerAddress,
                        notes = booking.Notes,
                        tourName = booking.Tour?.TourName,
                        departureDate = booking.DepartureDate.ToString("dd/MM/yyyy"),
                        totalPrice = booking.TotalPrice,
                        adultCount = booking.AdultCount,
                        childCount = booking.ChildCount,
                        infantCount = booking.InfantCount,
                        babyCount = booking.BabyCount,
                        singleRoomCount = booking.SingleRoomCount,
                        paymentMethod = booking.PaymentMethod,
                        paymentStatus = booking.PaymentStatus,
                        status = GetStatusText(booking.Status)
                    }
                };

                return Json(details);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: /Booking/CancelBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                // Kiểm tra đăng nhập
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                // Lấy booking của user hiện tại
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng hoặc bạn không có quyền hủy!" });
                }

                // Kiểm tra trạng thái có thể hủy
                if (booking.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "Đơn hàng đã bị hủy trước đó!" });
                }

                if (booking.Status == "Confirmed" || booking.Status == "Completed")
                {
                    return Json(new { success = false, message = "Không thể hủy đơn hàng đã xác nhận hoặc hoàn thành!" });
                }

                // Hủy đơn hàng
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã hủy đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Helper method để convert status
        private string GetStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "Cancelled" => "Đã hủy",
                "Completed" => "Hoàn thành",
                _ => "Không xác định"
            };
        }
    }

    // ViewModel for Booking Checkout
    public class BookingCheckoutViewModel
    {
        public Tour Tour { get; set; } = new Tour();
        public User User { get; set; } = new User();
        public int TourId { get; set; }

        // Departure Date
        public DateTime SelectedDepartureDate { get; set; }

        // Contact Information
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = "";
        public string? Notes { get; set; }

        // Passenger Counts
        public int AdultCount { get; set; } = 1;
        public int ChildCount { get; set; } = 0;
        public int InfantCount { get; set; } = 0;
        public int BabyCount { get; set; } = 0;

        // Room Supplement
        public bool SingleRoomSupplement { get; set; } = false;
        public int SingleRoomCount { get; set; } = 0;

        // Payment Method
        public string PaymentMethod { get; set; } = "BankTransfer";
    }

    // ViewModel for Payment Page
    public class PaymentViewModel
    {
        public int BookingId { get; set; }
        public Tour Tour { get; set; } = new Tour();
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public DateTime DepartureDate { get; set; }
        public int TotalPassengers { get; set; }
    }
}
