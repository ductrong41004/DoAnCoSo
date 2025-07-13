namespace TourDuLich.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }   // sửa
        public DateTime ReviewDate { get; set; }

        public User? User { get; set; }        // sửa
        public Tour? Tour { get; set; }        // sửa
    }
}
