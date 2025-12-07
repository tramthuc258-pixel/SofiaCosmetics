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
        public int MaCTSP { get; set; }   // ✅ thêm để biết biến thể

        public string TenSP { get; set; }
        public string ThuongHieu { get; set; }

        public decimal? GiaGoc { get; set; }
        public double? PhanTramGiam { get; set; }
        public decimal? GiaSauGiam { get; set; }

        public int? TonKho { get; set; }
        public string HinhAnh { get; set; }
        public bool? TrangThai { get; set; }

        public string TenBienThe { get; set; }
    }

    // Model add
    public class AddProductModel
    {
        public string TenSP { get; set; }
        public string MoTa { get; set; }
        public int MaDM { get; set; }
        public int MaTH { get; set; }
        public int? MaKM { get; set; }
        public decimal Gia { get; set; }
        public int TonKho { get; set; }
        public bool TrangThai { get; set; }
        public string TenBienThe { get; set; }
    }

    // Model edit
    public class EditProductModel : AddProductModel
    {
        public int MaSP { get; set; }
    }

    // item ảnh trả về cho View/Edit (cũ - vẫn giữ nếu bạn dùng chỗ khác)
    public class ProductImageVM
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }

    // ✅ Ảnh kèm biến thể để View chi tiết
    public class VariantImageVM
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public int MaCTSP { get; set; }
        public string TenBienThe { get; set; }

        public decimal Gia { get; set; }
        public int TonKho { get; set; }
    }

    // ✅ model thêm biến thể có khuyến mãi
    public class AddVariantModel
    {
        public int MaSP { get; set; }
        public string TenBienThe { get; set; }
        public decimal Gia { get; set; }
        public int TonKho { get; set; }
        public int? MaKM { get; set; }  // khuyến mãi riêng của biến thể
    }
}