using System.ComponentModel.DataAnnotations;

namespace TourDuLich.Models
{
    public class TourSchedule
    {
        public int TourScheduleId { get; set; }                // Primary Key
        
        [Required]
        public int TourId { get; set; }                        // Foreign Key to Tour
        
        [Required]
        public DateTime DepartureDate { get; set; }            // Ngày khởi hành cụ thể
        
        [Required]
        [Range(1, 1000)]
        public int AvailableSeats { get; set; }                // Số chỗ còn lại cho đợt này
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }                     // Giá cho đợt này (có thể khác nhau)
        
        public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;  // Trạng thái đợt khởi hành
        
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo
        
        public DateTime? UpdatedAt { get; set; }               // Ngày cập nhật
        
        // Navigation Properties
        public Tour Tour { get; set; } = null!;                // Liên kết với Tour
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); // Các booking cho đợt này
        
        // Computed Properties
        public bool IsLastMinute => (DepartureDate - DateTime.Now).TotalDays <= 1 && Status == ScheduleStatus.Active;
        public bool IsExpired => DepartureDate < DateTime.Now;
        public bool IsFull => AvailableSeats <= 0;
        public bool IsBookable => !IsExpired && !IsFull && Status == ScheduleStatus.Active;
        
        // Thời gian còn lại đến khởi hành (cho countdown)
        public TimeSpan TimeUntilDeparture => DepartureDate - DateTime.Now;
        
        // Số ngày còn lại
        public int DaysUntilDeparture => Math.Max(0, (int)(DepartureDate - DateTime.Now).TotalDays);
        
        // Số giờ còn lại (cho countdown)
        public int HoursUntilDeparture => Math.Max(0, (int)(DepartureDate - DateTime.Now).TotalHours);
    }
    
    public enum ScheduleStatus
    {
        Active = 0,      // Đang hoạt động
        Cancelled = 1,   // Đã hủy
        Completed = 2,   // Đã hoàn thành
        Full = 3         // Đã đầy chỗ
    }
}
