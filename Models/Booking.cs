namespace TourDuLich.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; } // sửa

        public User? User { get; set; }     // sửa
        public Tour? Tour { get; set; }     // sửa
    }
}
