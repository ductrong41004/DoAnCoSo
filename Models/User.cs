using System;
using System.Collections.Generic;

namespace TourDuLich.Models
{
    public enum UserType
    {
        Customer, // Khách hàng thường 0
        VIP,      // Khách hàng VIP 1 
        Admin     // Quản trị viên2 
    }

    public class User
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserType Type { get; set; } = UserType.Customer;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
