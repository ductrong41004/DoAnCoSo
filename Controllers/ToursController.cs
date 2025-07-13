using Microsoft.AspNetCore.Mvc;
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

        // Giao diện Razor View
        public IActionResult Index()
        {
            var tours = _context.Tours.ToList();
            return View(tours); // Sẽ tự động map tới Views/Tours/Index.cshtml
        }
    }
}
