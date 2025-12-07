using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    public class AdminAnalyticsVM
    {
        // summary cards
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int TongKhachHang { get; set; }
        public int TongSanPham { get; set; }

        // charts
        public List<RevenuePoint> DoanhThuTheoThang { get; set; }
        public List<CategorySalePoint> BanTheoDanhMuc { get; set; }
    }

    public class RevenuePoint
    {
        public string Thang { get; set; }   // "MM/yyyy"
        public decimal DoanhThu { get; set; }
    }

    public class CategorySalePoint
    {
        public string TenDM { get; set; }
        public decimal TyLe { get; set; }   // %
        public decimal DoanhThu { get; set; }
    }
}