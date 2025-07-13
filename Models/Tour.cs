namespace TourDuLich.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public string? TourName { get; set; }   
        public string? Description { get; set; } 
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }     
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }  
        public DateTime CreatedAt { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); 
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
