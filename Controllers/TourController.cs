using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TourDuLich.Data;
using TourDuLich.Models;

namespace TourDuLich.Controllers
{
    public class TourController : Controller
    {
        private readonly TourDuLichContext _context;

        // Constructor để inject DbContext vào controller
        public TourController(TourDuLichContext context)
        {
            _context = context;
        }

        // Action để hiển thị chi tiết tour
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            // Truy vấn để lấy thông tin tour theo ID
            var tour = await _context.Tours
                .Include(t => t.Bookings)  // Nếu cần thông tin đặt tour
                .Include(t => t.Reviews)   // Nếu cần thông tin đánh giá
                .FirstOrDefaultAsync(t => t.TourId == id);  // Lọc theo ID chuyến tour

            if (tour == null)
            {
                return NotFound();  // Nếu không tìm thấy chuyến tour, trả về lỗi 404
            }

            return View(tour);  // Trả về View chi tiết chuyến tour
        }
    }
}
