using Microsoft.EntityFrameworkCore;
using TourDuLich.Models;

namespace TourDuLich.Data
{
    public class TourDuLichContext : DbContext
    {
        public TourDuLichContext(DbContextOptions<TourDuLichContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
