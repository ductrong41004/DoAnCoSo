using Microsoft.AspNetCore.Mvc;
using TourDuLich.Data;
using TourDuLich.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TourDuLich.Controllers
{
    public class AdminTourController : Controller
    {
        private readonly TourDuLichContext _context;

        public AdminTourController(TourDuLichContext context)
        {
            _context = context;
        }

        // GET: /AdminTour/Index - Trang danh sach tour
        public IActionResult Index()
        {
            // Kiểm tra quyền truy cập (CHỈ Admin = 2)
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");

            // ✅ THÊM DEBUG
            System.Diagnostics.Debug.WriteLine($"AdminTour Index - UserTypeInt: {currentUserType}");

            // Truyền UserType vào ViewBag để View kiểm tra
            ViewBag.CurrentUserType = currentUserType;

            // Kiểm tra quyền (CHỈ Admin = 2)
            if (currentUserType != 2)
            {
                // Nếu không phải Admin, trả về danh sách rỗng
                return View(new List<Tour>());
            }

            var tours = _context.Tours.ToList();
            return View(tours); // Views/AdminTour/Index.cshtml
        }

        // GET: /AdminTour/Create
        public IActionResult Create()
        {
            // Kiểm tra quyền truy cập (VIP = 1, Admin = 2)
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");

            // ✅ THÊM DEBUG
            System.Diagnostics.Debug.WriteLine($"AdminTour Create - UserTypeInt: {currentUserType}");

            // Truyền UserType vào ViewBag để View kiểm tra
            ViewBag.CurrentUserType = currentUserType;

            // Kiểm tra quyền (VIP = 1 hoặc Admin = 2)
            if (currentUserType != 1 && currentUserType != 2)
            {
                // Nếu không phải VIP hoặc Admin, trả về view với thông báo lỗi
                return View();
            }

            return View(); // Views/AdminTour/Create.cshtml
        }

        // POST: /AdminTour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour, List<IFormFile> imageFiles)
        {
            // Kiểm tra quyền trước khi tạo
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            if (currentUserType != 1 && currentUserType != 2)
            {
                return Forbid(); // Trả về lỗi 403 nếu không có quyền
            }

            // ✅ XỬ LÝ NHIỀU NGÀY KHỞI HÀNH VỚI SỐ CHỖ
            var departureDatesArray = Request.Form["DepartureDates"].ToArray();
            var departureSeatsArray = Request.Form["DepartureSeats"].ToArray();

            var departureDatesList = new List<string>();

            if (departureDatesArray != null && departureDatesArray.Length > 0)
            {
                for (int i = 0; i < departureDatesArray.Length; i++)
                {
                    var date = departureDatesArray[i];
                    var seats = i < departureSeatsArray.Length ? departureSeatsArray[i] : "30"; // Default 30 chỗ

                    if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(seats))
                    {
                        // Parse datetime và chỉ lấy phần ngày
                        if (DateTime.TryParse(date, out var parsedDate))
                        {
                            // Format: "2025-08-14:30" (chỉ ngày, không có giờ)
                            departureDatesList.Add($"{parsedDate:yyyy-MM-dd}:{seats}");
                        }
                    }
                }
            }

            // ✅ NẾU KHÔNG CÓ NGÀY KHỞI HÀNH, TẠO MẶC ĐỊNH
            if (departureDatesList.Count == 0)
            {
                // Tạo 5 ngày khởi hành mẫu (mỗi tuần 1 chuyến, mỗi chuyến 30 chỗ)
                for (int i = 1; i <= 5; i++)
                {
                    var futureDate = DateTime.Now.AddDays(i * 7);
                    departureDatesList.Add($"{futureDate:yyyy-MM-dd}:30");
                }
            }

            tour.DepartureDates = string.Join(";", departureDatesList);

            if (ModelState.IsValid)
            {
                // ✅ TỰ ĐỘNG SET CreatedAt
                tour.CreatedAt = DateTime.Now;

                var imagePaths = new List<string>();

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    int count = 0;
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            if (count >= 5) break;
                            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                            if (!Directory.Exists(uploads))
                                Directory.CreateDirectory(uploads);

                            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploads, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            imagePaths.Add("/images/" + fileName);
                            count++;
                        }
                    }
                }

                tour.ImageUrl = string.Join(";", imagePaths); // Lưu tất cả ảnh vào cơ sở dữ liệu

                _context.Tours.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // ✅ Truyền ViewBag khi có lỗi
            ViewBag.CurrentUserType = currentUserType;
            return View(tour);
        }

        // GET: /AdminTour/Edit/5
        public IActionResult Edit(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();
            return View(tour); // Views/AdminTour/Edit.cshtml
        }

        // POST: /AdminTour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tour tour, List<IFormFile> imageFiles)
        {
            // ✅ XỬ LÝ NHIỀU NGÀY KHỞI HÀNH VỚI SỐ CHỖ
            var departureDatesArray = Request.Form["DepartureDates"].ToArray();
            var departureSeatsArray = Request.Form["DepartureSeats"].ToArray();

            // ✅ DEBUG: Log dữ liệu nhận được
            Console.WriteLine($"DEBUG - DepartureDates count: {departureDatesArray?.Length ?? 0}");
            Console.WriteLine($"DEBUG - DepartureSeats count: {departureSeatsArray?.Length ?? 0}");

            for (int i = 0; i < (departureDatesArray?.Length ?? 0); i++)
            {
                Console.WriteLine($"DEBUG - Date[{i}]: '{departureDatesArray[i]}'");
                if (i < (departureSeatsArray?.Length ?? 0))
                    Console.WriteLine($"DEBUG - Seats[{i}]: '{departureSeatsArray[i]}'");
            }

            var departureDatesList = new List<string>();

            if (departureDatesArray != null && departureDatesArray.Length > 0)
            {
                for (int i = 0; i < departureDatesArray.Length; i++)
                {
                    var date = departureDatesArray[i];
                    var seats = i < departureSeatsArray.Length ? departureSeatsArray[i] : "30"; // Default 30 chỗ

                    Console.WriteLine($"DEBUG - Processing: Date='{date}', Seats='{seats}'");

                    if (!string.IsNullOrEmpty(date))
                    {
                        // Nếu không có số chỗ, dùng mặc định 30
                        if (string.IsNullOrEmpty(seats))
                            seats = "30";

                        // Parse datetime và chỉ lấy phần ngày
                        if (DateTime.TryParse(date, out var parsedDate))
                        {
                            // Format: "2025-08-14:30" (chỉ ngày, không có giờ)
                            var formattedEntry = $"{parsedDate:yyyy-MM-dd}:{seats}";
                            departureDatesList.Add(formattedEntry);
                            Console.WriteLine($"DEBUG - Added: '{formattedEntry}'");
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG - Failed to parse date: '{date}'");
                        }
                    }
                }
            }

            tour.DepartureDates = string.Join(";", departureDatesList);
            Console.WriteLine($"DEBUG - Final DepartureDates: '{tour.DepartureDates}'");

            if (ModelState.IsValid)
            {
                // ✅ LẤY TOUR CŨ TỪ DATABASE
                var existingTour = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.TourId == tour.TourId);
                if (existingTour == null)
                {
                    return NotFound();
                }

                // ✅ GIỮ NGUYÊN CreatedAt VÀ ImageUrl CŨ
                tour.CreatedAt = existingTour.CreatedAt;

                // ✅ XỬ LÝ ẢNH MỚI
                if (imageFiles != null && imageFiles.Count > 0 && imageFiles.Any(f => f.Length > 0))
                {
                    // Có ảnh mới được upload
                    var imagePaths = new List<string>();
                    int count = 0;

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            if (count >= 5) break;
                            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                            if (!Directory.Exists(uploads))
                                Directory.CreateDirectory(uploads);

                            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploads, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            imagePaths.Add("/images/" + fileName);
                            count++;
                        }
                    }

                    // Cập nhật ảnh mới
                    tour.ImageUrl = string.Join(";", imagePaths);
                }
                else
                {
                    // Không có ảnh mới, giữ nguyên ảnh cũ
                    tour.ImageUrl = existingTour.ImageUrl;
                }

                _context.Tours.Update(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: /AdminTour/Delete/5
        public IActionResult Delete(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();
            return View(tour); // Views/AdminTour/Delete.cshtml
        }

        // POST: /AdminTour/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();
            _context.Tours.Remove(tour);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); // Sau khi xóa, quay lại danh sách
        }

        // GET: /AdminTour/Details/5
        public IActionResult Details(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();
            return View(tour); // Views/AdminTour/Details.cshtml
        }

        // ===============================
        // QUẢN LÝ PHÂN QUYỀN USER
        // ===============================

        // GET: /AdminTour/User - ĐÃ THÊM PHÂN QUYỀN
        public async Task<IActionResult> User()
        {
            // Lấy thông tin user hiện tại từ session
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");

            // ✅ THÊM DEBUG
            System.Diagnostics.Debug.WriteLine($"Current UserTypeInt from Session: {currentUserType}");

            // Truyền UserType vào ViewBag để View kiểm tra
            ViewBag.CurrentUserType = currentUserType;

            // Kiểm tra quyền Admin (Type = 2)
            if (currentUserType != 2)
            {
                // Nếu không phải Admin, trả về danh sách rỗng (View sẽ hiển thị thông báo lỗi)
                return View(new List<User>());
            }

            // Nếu là Admin, lấy danh sách user
            var users = await _context.Users.ToListAsync();
            return View(users); // Views/AdminTour/User.cshtml
        }

        // POST: /AdminTour/UpdateUserType
        [HttpPost]
        public async Task<IActionResult> UpdateUserType(int userId, int type)
        {
            // Kiểm tra quyền Admin trước khi cập nhật
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            if (currentUserType != 2)
            {
                return Forbid(); // Trả về lỗi 403 nếu không phải Admin
            }

            System.Diagnostics.Debug.WriteLine($"UpdateUserType called. userId={userId}, type={type}");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.Type = (UserType)type;
            await _context.SaveChangesAsync();

            return RedirectToAction("User");
        }

        // ===============================
        // QUẢN LÝ ĐƠN ĐẶT TOUR
        // ===============================

        // GET: /AdminTour/ManageBooking
        [HttpGet]
        public async Task<IActionResult> ManageBooking()
        {
            // Kiểm tra quyền admin
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            ViewBag.CurrentUserType = currentUserType;

            if (currentUserType != 2)
            {
                // Nếu không phải Admin, trả về danh sách rỗng
                return View(new List<Booking>());
            }

            // Lấy tất cả booking với thông tin tour và user
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // POST: /AdminTour/ConfirmBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tour)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                if (booking.Status != "Pending")
                {
                    return Json(new { success = false, message = "Đơn hàng này không thể xác nhận!" });
                }

                // ✅ KHÔNG TRỪ SỐ CHỖ KHI XÁC NHẬN - ĐÃ TRỪ KHI TẠO BOOKING PENDING
                // Chỉ cần đổi status từ Pending -> Confirmed
                // Số chỗ đã được tính trong logic GetAvailableSeatsForDate() rồi

                booking.Status = "Confirmed";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xác nhận đơn hàng và cập nhật số chỗ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: /AdminTour/CancelBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tour)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                if (booking.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "Đơn hàng đã bị hủy trước đó!" });
                }

                // ✅ KHÔNG CẦN HOÀN TRẢ SỐ CHỖ - SỐ CHỖ ĐƯỢC TÍNH ĐỘNG QUA BOOKING STATUS
                // Logic GetAvailableSeatsForDate() sẽ tự động tính số chỗ còn lại dựa trên booking status

                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã hủy đơn hàng và hoàn trả số chỗ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: /AdminTour/GetBookingDetails
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tour)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                var details = new
                {
                    success = true,
                    data = new
                    {
                        bookingId = booking.BookingId,
                        customerName = booking.CustomerName,
                        customerEmail = booking.CustomerEmail,
                        customerPhone = booking.CustomerPhone,
                        customerAddress = booking.CustomerAddress,
                        notes = booking.Notes,
                        tourName = booking.Tour?.TourName,
                        departureDate = booking.DepartureDate.ToString("dd/MM/yyyy"),
                        totalPrice = booking.TotalPrice.ToString("N0"),
                        adultCount = booking.AdultCount,
                        childCount = booking.ChildCount,
                        infantCount = booking.InfantCount,
                        babyCount = booking.BabyCount,
                        singleRoomCount = booking.SingleRoomCount,
                        paymentMethod = booking.PaymentMethod,
                        paymentStatus = booking.PaymentStatus
                    }
                };

                return Json(details);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: /AdminTour/ViewItinerary/5 - Xem hanh trinh chi tiet
        public async Task<IActionResult> ViewItinerary(int id)
        {
            // Kiem tra quyen truy cap
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            if (currentUserType != 2) // Chi Admin
            {
                return RedirectToAction("Index", "Tours");
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // GET: /AdminTour/EditItinerary/5 - Form cap nhat hanh trinh
        public async Task<IActionResult> EditItinerary(int id)
        {
            // Kiem tra quyen truy cap
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            if (currentUserType != 2) // Chi Admin
            {
                return RedirectToAction("Index", "Tours");
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: /AdminTour/EditItinerary/5 - Xu ly cap nhat hanh trinh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItinerary(int id, string itinerary)
        {
            // Kiem tra quyen truy cap
            var currentUserType = HttpContext.Session.GetInt32("UserTypeInt");
            if (currentUserType != 2) // Chi Admin
            {
                return RedirectToAction("Index", "Tours");
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            try
            {
                tour.Itinerary = itinerary;
                _context.Update(tour);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cap nhat hanh trinh thanh cong!";
                return RedirectToAction("ViewItinerary", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Co loi xay ra: " + ex.Message;
                return View(tour);
            }
        }
    }
}