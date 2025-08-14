using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TourDuLich.Data;
using TourDuLich.Models;

namespace TourDuLich.Controllers
{
    public class ToursController : Controller
    {
        private readonly TourDuLichContext _context;

        public ToursController(TourDuLichContext context)
        {
            _context = context;
        }

        // ✅ ACTION INDEX CHO TRANG CHỦ - CO HO TRO FILTER THEO CATEGORY
        public async Task<IActionResult> Index(string? category)
        {
            var currentDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var urgentDate = currentDate.AddDays(7); // 7 ngày tới

            // ✅ 1. Tour Giờ Chót - Tours có ngày khởi hành trong 7 ngày tới
            var allTours = await _context.Tours.ToListAsync();
            var urgentTours = allTours
                .Where(t => t.HasLastMinuteDeparture()) // Sử dụng helper method mới
                .OrderBy(t => t.GetNextDepartureDate()) // Sắp xếp theo ngày khởi hành gần nhất
                .ToList();

            // Neu co filter theo category, chi hien thi category do
            if (!string.IsNullOrEmpty(category) && Enum.TryParse<TourCategory>(category, out var selectedCategory))
            {
                var filteredTours = await _context.Tours
                    .Where(t => t.Category == selectedCategory)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();

                var filteredViewModel = new ToursIndexViewModel
                {
                    UrgentTours = new List<Tour>(), // Khong hien thi urgent khi filter
                    MienBacTours = selectedCategory == TourCategory.MienBac ? filteredTours : new List<Tour>(),
                    MienTrungTours = selectedCategory == TourCategory.MienTrung ? filteredTours : new List<Tour>(),
                    MienNamTours = selectedCategory == TourCategory.MienNam ? filteredTours : new List<Tour>(),
                    ChauATours = selectedCategory == TourCategory.ChauA ? filteredTours : new List<Tour>(),
                    AuUcMyPhiTours = selectedCategory == TourCategory.AuUcMyPhi ? filteredTours : new List<Tour>(),
                    SelectedCategory = category
                };

                return View(filteredViewModel);
            }

            // ✅ 2. Tour Mien Bac - Category = 0
            var mienBacTours = await _context.Tours
                .Where(t => t.Category == TourCategory.MienBac)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            // ✅ 3. Tour Mien Trung - Category = 1
            var mienTrungTours = await _context.Tours
                .Where(t => t.Category == TourCategory.MienTrung)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            // ✅ 4. Tour Mien Nam - Category = 2
            var mienNamTours = await _context.Tours
                .Where(t => t.Category == TourCategory.MienNam)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            // ✅ 5. Tour Chau A - Category = 3
            var chauATours = await _context.Tours
                .Where(t => t.Category == TourCategory.ChauA)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            // ✅ 6. Tour Au-Uc-My-Phi - Category = 4
            var auUcMyPhiTours = await _context.Tours
                .Where(t => t.Category == TourCategory.AuUcMyPhi)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            // Tao ViewModel de truyen du lieu
            var viewModel = new ToursIndexViewModel
            {
                UrgentTours = urgentTours,
                MienBacTours = mienBacTours,
                MienTrungTours = mienTrungTours,
                MienNamTours = mienNamTours,
                ChauATours = chauATours,
                AuUcMyPhiTours = auUcMyPhiTours,
                SelectedCategory = category
            };

            return View(viewModel);
        }

        // Action để hiển thị chi tiết tour
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var tour = await _context.Tours
                    .Include(t => t.Bookings)
                    .Include(t => t.Reviews)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(t => t.TourId == id);

                if (tour == null)
                {
                    return NotFound();
                }

                return View(tour);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Error in Detail action: {ex.Message}");

                // Thử load tour đơn giản không có Reviews
                var simpleTour = await _context.Tours
                    .FirstOrDefaultAsync(t => t.TourId == id);

                if (simpleTour == null)
                {
                    return NotFound();
                }

                return View(simpleTour);
            }
        }

        // API: GET Tours/GetRelatedTours
        [HttpGet]
        public async Task<IActionResult> GetRelatedTours(int currentTourId)
        {
            try
            {
                // Lấy tất cả tour khác (không bao gồm tour hiện tại)
                var relatedTours = await _context.Tours
                    .Where(t => t.TourId != currentTourId)
                    .OrderBy(t => Guid.NewGuid()) // Random order
                    .Take(8) // Lấy tối đa 8 tour
                    .Select(t => new
                    {
                        t.TourId,
                        t.TourName,
                        t.Location,
                        t.Duration,
                        t.Price,
                        AvailableSeats = t.GetDepartureDatesWithSeats().Values.Sum(),
                        t.ImageUrl,
                        t.Category
                    })
                    .ToListAsync();

                return Json(relatedTours);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Không thể tải tour liên quan", details = ex.Message });
            }
        }

        // ✅ ACTION CẬP NHẬT DỮ LIỆU DATABASE - THÊM SỐ CHỖ CHO MỖI NGÀY
        [HttpGet]
        public async Task<IActionResult> UpdateDatabaseFormat()
        {
            try
            {
                var tours = await _context.Tours.ToListAsync();
                int updatedCount = 0;

                foreach (var tour in tours)
                {
                    if (!string.IsNullOrEmpty(tour.DepartureDates))
                    {
                        // Kiểm tra xem đã có format mới chưa (có dấu ":")
                        if (!tour.DepartureDates.Contains(":"))
                        {
                            // Convert từ format cũ "2025-08-15T00:00;2025-08-18T00:00"
                            // sang format mới "2025-08-15:30;2025-08-18:30"
                            var oldDates = tour.DepartureDates.Split(';')
                                .Where(d => !string.IsNullOrEmpty(d))
                                .ToList();

                            var newDatesWithSeats = new List<string>();

                            foreach (var dateStr in oldDates)
                            {
                                // Parse ngày từ format "2025-08-15T00:00"
                                if (DateTime.TryParse(dateStr, out var date))
                                {
                                    // Thêm 30 chỗ cho mỗi ngày
                                    newDatesWithSeats.Add($"{date:yyyy-MM-dd}:30");
                                }
                            }

                            if (newDatesWithSeats.Any())
                            {
                                tour.DepartureDates = string.Join(";", newDatesWithSeats);
                                updatedCount++;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new {
                    success = true,
                    message = $"✅ Đã cập nhật {updatedCount} tour thành công! Mỗi ngày khởi hành có 30 chỗ.",
                    updatedCount = updatedCount,
                    totalTours = tours.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new {
                    success = false,
                    message = $"❌ Lỗi khi cập nhật: {ex.Message}"
                });
            }
        }
    }

    // ✅ ViewModel de truyen du lieu cho View
    public class ToursIndexViewModel
    {
        public List<Tour> UrgentTours { get; set; } = new List<Tour>();
        public List<Tour> MienBacTours { get; set; } = new List<Tour>();
        public List<Tour> MienTrungTours { get; set; } = new List<Tour>();
        public List<Tour> MienNamTours { get; set; } = new List<Tour>();
        public List<Tour> ChauATours { get; set; } = new List<Tour>();
        public List<Tour> AuUcMyPhiTours { get; set; } = new List<Tour>();
        public string? SelectedCategory { get; set; }
    }
}