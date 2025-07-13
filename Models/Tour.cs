namespace TourDuLich.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public string? TourName { get; set; }      // sửa
        public string? Description { get; set; }   // sửa
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }      // sửa
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }      // sửa
        public DateTime CreatedAt { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); // khởi tạo mặc định
        public ICollection<Review> Reviews { get; set; } = new List<Review>();    // khởi tạo mặc định
    }
}
