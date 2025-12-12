using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    // Item list cho Index
    public class SanPhamAdmin
    {
        public int MaSP { get; set; }
        public int MaCTSP { get; set; }   // ✅ biết biến thể nào

        public string TenSP { get; set; }
        public string ThuongHieu { get; set; }

        public decimal? GiaGoc { get; set; }
        public double? PhanTramGiam { get; set; }
        public decimal? GiaSauGiam { get; set; }

        public int? TonKho { get; set; }
        public string HinhAnh { get; set; }
        public bool? TrangThai { get; set; }

        public string TenBienThe { get; set; }
        public int? MaKM { get; set; } // ✅ KM theo biến thể
    }

    // Model add
    public class AddProductModel
    {
        public string TenSP { get; set; }
        public string MoTa { get; set; }
        public int MaDM { get; set; }
        public int MaTH { get; set; }

        public decimal Gia { get; set; }
        public int TonKho { get; set; }

        public bool TrangThai { get; set; }
        public string TenBienThe { get; set; }

        public int? MaKM { get; set; } // ✅ KM theo biến thể
    }

    // Model edit (edit từng biến thể)
    public class EditProductModel : AddProductModel
    {
        public int MaSP { get; set; }
        public int MaCTSP { get; set; } // ✅ edit đúng dòng
    }

    // ✅ Ảnh trả về có biến thể
    public class VariantImageVM
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public int MaCTSP { get; set; }
        public string TenBienThe { get; set; }

        public decimal Gia { get; set; }
        public int TonKho { get; set; }
    }

    // ✅ model thêm biến thể
    public class AddVariantModel
    {
        public int MaSP { get; set; }
        public string TenBienThe { get; set; }
        public decimal Gia { get; set; }
        public int TonKho { get; set; }
        public int? MaKM { get; set; } // ✅ KM riêng biến thể
    }

}