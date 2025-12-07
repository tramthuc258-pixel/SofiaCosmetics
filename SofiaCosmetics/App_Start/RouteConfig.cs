using System.Web.Mvc;
using System.Web.Routing;

namespace SofiaCosmetics
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // =========================================================
            // 0) NHÓM ROUTE CỤ THỂ DƯỚI /danh-muc/...
            // =========================================================

            // Trang chủ trong danh-muc
            routes.MapRoute(
                name: "HomeInDanhMuc",
                url: "danh-muc/trang-chu",
                defaults: new { controller = "Home", action = "Index" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );

            // Sản phẩm trong danh-muc
            routes.MapRoute(
                name: "SanPhamInDanhMuc",
                url: "danh-muc/san-pham",
                defaults: new { controller = "Home", action = "SanPham" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );

            // Tin tức trong danh-muc  ✅ FIX 404
            routes.MapRoute(
                name: "TinTucInDanhMuc",
                url: "danh-muc/tin-tuc",
                defaults: new { controller = "TinTuc", action = "Index" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );

            // Khuyến mãi trong danh-muc
            routes.MapRoute(
                name: "KhuyenMaiInDanhMuc",
                url: "danh-muc/khuyen-mai",
                defaults: new { controller = "KhuyenMai", action = "Index" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 1) CÁC TRANG TĨNH dưới /danh-muc/{slug}
            // =========================================================
            routes.MapRoute(
                name: "StaticInDanhMuc",
                url: "danh-muc/{slug}",
                defaults: new { controller = "TinTuc", action = "Slug" },
                constraints: new
                {
                    slug = @"^(gioi-thieu|lien-he|chinh-sach-giao-hang|chinh-sach-doi-tra|chinh-sach-bao-mat|dieu-khoan-su-dung|huong-dan-mua-hang)$"
                },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 2) DANH MỤC SẢN PHẨM /danh-muc/{link}
            // (đặt sau static để không nuốt static)
            // =========================================================
            routes.MapRoute(
                name: "DanhMuc",
                url: "danh-muc/{link}",
                defaults: new { controller = "Home", action = "DanhMuc", link = UrlParameter.Optional },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 3) ROUTE TIN TỨC NGOÀI /tin-tuc
            // =========================================================

            // Trang list tin tức: /tin-tuc
            routes.MapRoute(
                name: "TinTucIndex",
                url: "tin-tuc",
                defaults: new { controller = "TinTuc", action = "Index" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );

            // Chi tiết tin tức: /tin-tuc/{slug}
            routes.MapRoute(
                name: "TinTucChiTiet",
                url: "tin-tuc/{slug}",
                defaults: new { controller = "TinTuc", action = "ChiTiet", slug = UrlParameter.Optional },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 4) HOME gốc
            // =========================================================
            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 5) DEFAULT
            // =========================================================
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );


            // =========================================================
            // 6) SLUG 1 cấp còn lại (phải để CUỐI)
            // =========================================================
            routes.MapRoute(
                name: "NewsSlug",
                url: "{slug}",
                defaults: new { controller = "TinTuc", action = "Slug" },
                constraints: new { slug = @"^[a-z0-9-]+$" },
                namespaces: new[] { "SofiaCosmetics.Controllers" }
            );
        }
    }
}
