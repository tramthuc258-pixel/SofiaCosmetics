using System;
using System.Collections.Generic;
using System.Linq;

namespace SofiaCosmetics.Areas.Admin.Helpers
{
    // Danh sách module (tương ứng tên Controller)
    public static class AdminModules
    {
        public const string TrangChu = "TrangChu";
        public const string SanPham = "SanPham";
        public const string DonHang = "DonHang";
        public const string KhachHang = "KhachHang";
        public const string Menu = "Menu";
        public const string ThuongHieu = "ThuongHieu";
        public const string KhuyenMai = "KhuyenMai";
        public const string TinTuc = "TinTuc";
        public const string Slider = "Slider";
        public const string PhanTich = "PhanTich";
        public const string NhanSu = "NhanSu";
        public const string Audit = "Audit"; // nếu có trang xem log

        public static readonly List<string> All = new List<string>
        {
            TrangChu, SanPham, DonHang, KhachHang,
            Menu, ThuongHieu, KhuyenMai, TinTuc,
            Slider, PhanTich, NhanSu, Audit
        };
    }

    public static class PermissionConfig
    {
        // QUYỀN THEO VAI TRÒ (VaiTro trong bảng ADMIN)
        private static readonly Dictionary<string, List<string>> RolePermissions =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Quản lý: full quyền
                { "Quản lý", AdminModules.All },

                // Nhân viên: ví dụ chỉ được làm việc với đơn hàng + xem Trang chủ
                { "Nhân viên", new List<string>
                    {
                        AdminModules.TrangChu,
                        AdminModules.DonHang
                    }
                },
            };

        // OVERRIDE THEO USER CỤ THỂ (TenDangNhap)
        private static readonly Dictionary<string, List<string>> UserPermissions =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Ví dụ: nhanvien1 chỉ được TrangChu + DonHang
                { "nhanvien1", new List<string>
                    {
                        AdminModules.TrangChu,
                        AdminModules.DonHang,
                        AdminModules.KhachHang,
                        AdminModules.PhanTich
                    }
                },

                // Ví dụ: nhanvien2 được thêm SanPham + KhuyenMai
                { "nhanvien2", new List<string>
                    {
                        AdminModules.TrangChu,
                        AdminModules.SanPham,
                        AdminModules.ThuongHieu,
                        AdminModules.KhuyenMai
                    }
                },

                { "nhanvien3", new List<string> 
                    {
                        AdminModules.TrangChu,
                        AdminModules.TinTuc,
                        AdminModules.Slider,
                        AdminModules.Menu
                    }
                },

                // Thích custom cho ai thì add thêm ở đây
            };

        // Lấy danh sách module được phép của 1 admin
        public static List<string> GetAllowedModules(string vaiTro, string tenDangNhap)
        {
            // ƯU TIÊN: nếu user có cấu hình riêng → dùng theo user
            if (!string.IsNullOrWhiteSpace(tenDangNhap) &&
                UserPermissions.ContainsKey(tenDangNhap))
            {
                return UserPermissions[tenDangNhap];
            }

            // Nếu không có override → dùng theo vai trò
            if (!string.IsNullOrWhiteSpace(vaiTro) &&
                RolePermissions.ContainsKey(vaiTro))
            {
                return RolePermissions[vaiTro];
            }

            // Không biết gì hết → chỉ cho vào Trang chủ
            return new List<string> { AdminModules.TrangChu };
        }

        // Check controller hiện tại có nằm trong quyền không
        public static bool HasPermission(List<string> allowedModules, string controllerName)
        {
            if (allowedModules == null) return false;
            if (string.IsNullOrWhiteSpace(controllerName)) return false;

            return allowedModules.Any(m =>
                m.Equals(controllerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
