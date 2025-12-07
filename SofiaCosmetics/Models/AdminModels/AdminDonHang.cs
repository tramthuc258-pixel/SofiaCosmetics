using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    // Dùng cho danh sách đơn hàng (Index)
    public class AdminOrderListVM
    {
        public int MaDH { get; set; }
        public string KhachHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    // Dùng cho từng sản phẩm trong đơn
    public class AdminOrderDetailItemVM
    {
        public string TenSP { get; set; }
        public string TenBienThe { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string HinhAnh { get; set; }
    }

    // Dùng cho chi tiết đơn (header + list sản phẩm)
    public class AdminOrderDetailVM
    {
        public int MaDH { get; set; }
        public string KhachHang { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        public List<AdminOrderDetailItemVM> Items { get; set; }
    }

    // Dùng cho sửa trạng thái (AJAX)
    public class AdminOrderEditVM
    {
        public int MaDH { get; set; }
        public string TrangThai { get; set; }
    }
}