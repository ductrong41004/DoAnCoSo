namespace TourDuLich.Models
{
    public class DashboardViewModel
    {
        public int TongSoDonDat { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TourDangChoDuyet { get; set; }
        public List<Tour> SuKienSapToi { get; set; }
    }
}
