using System;
using System.Collections.Generic;
using System.Linq;
using TourDuLich.Data;

namespace TourDuLich.Models
{
    public class Tour
    {
        public int TourId { get; set; }                    // Mã Tour (Primary Key)
        public string? TourName { get; set; }              // Tên Tour
        public string? Description { get; set; }           // Mô tả Tour
        public decimal Price { get; set; }                 // Giá Tour
        public string? Duration { get; set; }              // Thời gian Tour (nvarchar(max))
        public string? Location { get; set; }              // Địa điểm Tour
        public string? Transportation { get; set; }        // ✅ Phương Tiện (nvarchar(max))
        public string? DepartureFrom { get; set; }         // ✅ Xuất phát Từ (nvarchar(max))
        public DateTime CreatedAt { get; set; }            // Ngày tạo Tour
        public string? DepartureDates { get; set; }        // Các ngày khởi hành với số chỗ (format: "date:seats;date:seats")
        public string? ImageUrl { get; set; }              // URL anh giu lai
        public TourCategory Category { get; set; } = TourCategory.MienBac; // Phan loai Tour
        public string? Itinerary { get; set; }             // Hanh trinh tung ngay JSON format

        // Navigation Properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Helper Methods cho nhiều ngày khởi hành với số chỗ
        public List<DateTime> GetDepartureDatesList()
        {
            if (string.IsNullOrEmpty(DepartureDates))
                return new List<DateTime>();

            var dates = new List<DateTime>();
            var dateStrings = DepartureDates.Split(';');

            foreach (var dateString in dateStrings)
            {
                if (string.IsNullOrEmpty(dateString))
                    continue;

                // Thử parse toàn bộ chuỗi trước
                if (DateTime.TryParse(dateString, out DateTime fullDate))
                {
                    dates.Add(fullDate);
                }
                else
                {
                    // Nếu không được, thử lấy phần date trước dấu ":"
                    var datePart = dateString.Split(':')[0];
                    if (DateTime.TryParse(datePart, out DateTime parsedDate))
                    {
                        dates.Add(parsedDate);
                    }
                }
            }

            return dates.OrderBy(d => d).ToList();
        }

        // Lấy danh sách ngày khởi hành với số chỗ
        public Dictionary<DateTime, int> GetDepartureDatesWithSeats()
        {
            if (string.IsNullOrEmpty(DepartureDates))
                return new Dictionary<DateTime, int>();

            var result = new Dictionary<DateTime, int>();
            var entries = DepartureDates.Split(';')
                .Where(d => !string.IsNullOrEmpty(d));

            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var date) && int.TryParse(parts[1], out var seats))
                {
                    result[date] = seats;
                }
            }

            return result.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        // Lấy số chỗ ban đầu cho ngày cụ thể (từ DepartureDates string)
        public int GetInitialSeatsForDate(DateTime date)
        {
            var datesWithSeats = GetDepartureDatesWithSeats();
            return datesWithSeats.ContainsKey(date.Date) ? datesWithSeats[date.Date] : 0;
        }

        // Lấy số chỗ còn lại cho ngày cụ thể (tính cả booking) - CẦN CONTEXT
        public int GetAvailableSeatsForDate(DateTime date)
        {
            // Để tương thích ngược, vẫn trả về số chỗ ban đầu
            // Logic tính booking sẽ được xử lý ở controller hoặc view
            return GetInitialSeatsForDate(date);
        }

        // Method mới để tính số chỗ còn lại với bookings đã load
        public int GetRealAvailableSeatsForDate(DateTime date)
        {
            var initialSeats = GetInitialSeatsForDate(date);

            // Tính số chỗ đã được đặt (CHỈ CONFIRMED) cho ngày này từ Bookings đã load
            var bookedSeats = this.Bookings?
                .Where(b => b.DepartureDate.Date == date.Date &&
                           b.Status == "Confirmed") // ✅ CHỈ TRỪ KHI ĐÃ XÁC NHẬN
                .Sum(b => b.AdultCount + b.ChildCount + b.InfantCount + b.BabyCount) ?? 0;

            return Math.Max(0, initialSeats - bookedSeats);
        }

        public DateTime? GetNextDepartureDate()
        {
            var dates = GetDepartureDatesList();
            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            return dates.FirstOrDefault(d => d > vietnamNow);
        }

        public DateTime? GetLastMinuteDepartureDate()
        {
            var dates = GetDepartureDatesList();
            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            return dates.FirstOrDefault(d => d > vietnamNow && (d - vietnamNow).TotalDays <= 7);
        }

        public bool HasLastMinuteDeparture()
        {
            return GetLastMinuteDepartureDate() != null;
        }

        public int GetDaysUntilNextDeparture()
        {
            var nextDate = GetNextDepartureDate();
            if (nextDate == null) return -1;

            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var timeSpan = nextDate.Value - vietnamNow;
            return Math.Max(0, (int)timeSpan.TotalDays);
        }

        public int GetHoursUntilNextDeparture()
        {
            var nextDate = GetNextDepartureDate();
            if (nextDate == null) return -1;

            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var timeSpan = nextDate.Value - vietnamNow;
            return Math.Max(0, (int)timeSpan.TotalHours % 24);
        }

        public int GetMinutesUntilNextDeparture()
        {
            var nextDate = GetNextDepartureDate();
            if (nextDate == null) return -1;

            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var timeSpan = nextDate.Value - vietnamNow;
            return Math.Max(0, (int)timeSpan.TotalMinutes % 60);
        }

        public string GetCountdownText()
        {
            var nextDate = GetNextDepartureDate();
            if (nextDate == null) return "Không có lịch khởi hành";

            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var timeSpan = nextDate.Value - vietnamNow;
            if (timeSpan.TotalDays < 0) return "Đã khởi hành";

            var days = (int)timeSpan.TotalDays;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;

            if (days > 0)
                return $"Còn {days} ngày {hours} giờ {minutes} phút";
            else if (hours > 0)
                return $"Còn {hours} giờ {minutes} phút";
            else
                return $"Còn {minutes} phút";
        }

        public TimeSpan? GetTimeUntilNextDeparture()
        {
            var nextDate = GetNextDepartureDate();
            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            return nextDate?.Subtract(vietnamNow);
        }

        // Method mới: Tính countdown cho ngày cụ thể
        public string GetCountdownTextForDate(DateTime specificDate)
        {
            var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            var timeSpan = specificDate - vietnamNow;
            if (timeSpan.TotalDays < 0) return "Đã khởi hành";

            var days = (int)timeSpan.TotalDays;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;

            if (days > 0)
                return $"Còn {days} ngày {hours} giờ {minutes} phút";
            else if (hours > 0)
                return $"Còn {hours} giờ {minutes} phút";
            else
                return $"Còn {minutes} phút";
        }

        // Helper methods cho hanh trinh
        public List<string> GetItineraryDays()
        {
            if (string.IsNullOrEmpty(Itinerary))
                return new List<string>();

            try
            {
                return Itinerary.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public string GetCategoryDisplayName()
        {
            switch (Category)
            {
                case TourCategory.MienBac:
                    return "Tour Mien Bac";
                case TourCategory.MienTrung:
                    return "Tour Mien Trung";
                case TourCategory.MienNam:
                    return "Tour Mien Nam";
                case TourCategory.ChauA:
                    return "Tour Du Lich Chau A";
                case TourCategory.AuUcMyPhi:
                    return "Tour Du Lich Au-Uc-My-Phi";
                default:
                    return "Khong xac dinh";
            }
        }

        // Compatibility voi code cu
        public DateTime DepartureDate => GetNextDepartureDate() ?? DateTime.MinValue;
    }

    public enum TourCategory
    {
        MienBac = 0,        // Tour Mien Bac
        MienTrung = 1,      // Tour Mien Trung
        MienNam = 2,        // Tour Mien Nam
        ChauA = 3,          // Tour Du Lich Chau A
        AuUcMyPhi = 4       // Tour Du Lich Au-Uc-My-Phi
    }
}