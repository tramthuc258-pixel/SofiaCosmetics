using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    public class AdminKhuyenMai
    {
        public int MaKM { get; set; }
        public string TenKhuyenMai { get; set; }
        public string MoTa { get; set; }
        public double? PhanTramGiam { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; }

        // thêm để xử lý hiển thị + filter
        public bool HetHan { get; set; }
    }

    public class AddKhuyenMaiModel
    {
        public string TenKhuyenMai { get; set; }
        public string MoTa { get; set; }
        public double? PhanTramGiam { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; }
    }

    public class EditKhuyenMaiModel
    {
        public int MaKM { get; set; }
        public string TenKhuyenMai { get; set; }
        public string MoTa { get; set; }
        public double? PhanTramGiam { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; }
    }
}