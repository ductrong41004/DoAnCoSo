namespace TourDuLich.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime DepartureDate { get; set; }  // Ngay khoi hanh duoc chon
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; } // Pending, Confirmed, Cancelled, Completed
        public string? PaymentMethod { get; set; } // BankTransfer, Crypto
        public string? PaymentStatus { get; set; } // Pending, Paid, Failed
        public DateTime? PaymentDate { get; set; } // Ngay thanh toan
        public string? TransactionId { get; set; } // Ma giao dich

        // Thong tin lien he
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? Notes { get; set; }

        // Chi tiet hanh khach
        public int AdultCount { get; set; } = 1;
        public int ChildCount { get; set; } = 0;
        public int InfantCount { get; set; } = 0;
        public int BabyCount { get; set; } = 0;
        public bool SingleRoomSupplement { get; set; } = false;
        public int SingleRoomCount { get; set; } = 0;

        public User? User { get; set; }     // Thuoc tinh dieu huong
        public Tour? Tour { get; set; }     // Thuoc tinh dieu huong
    }
}
