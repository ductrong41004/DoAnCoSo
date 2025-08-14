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

        // Phương thức để cấu hình các thuộc tính của Entity
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chỉ định kiểu cột cho các thuộc tính decimal
            modelBuilder.Entity<Tour>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");  // precision = 18, scale = 2

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");  // precision = 18, scale = 2
        }
    }
}
