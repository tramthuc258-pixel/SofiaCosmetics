using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.ViewModels
{
    public class SanPhamViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal? Gia { get; set; }
        public string HinhAnh { get; set; }

        public decimal? GiaKhuyenMai { get; set; }
        public double? PhanTramGiam { get; set; }
        public bool CoKhuyenMai => PhanTramGiam > 0; // Dễ kiểm tra trong View

        public int? MaCTSP { get; set; }
        //mới thêm
        public string TenBienThe { get; set; }     // vd: Màu, Size
        public int? SoLuongTon { get; set; }
    }

}