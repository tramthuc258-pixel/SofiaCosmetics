using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.ViewModels;

namespace SofiaCosmetics.Controllers
{
    public class CartController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();
        // GET: Cart
        // Lấy danh sách giỏ hàng trong Session
        private List<CartItem> LayGioHang()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // Xem giỏ hàng
        public ActionResult Index()
        {
            var cart = LayGioHang();
            decimal tamTinh = cart.Sum(x => x.ThanhTien);

            // Lấy % giảm đang có nếu đã áp dụng mã
            decimal discountPercent = Session["Discount"] != null ? (decimal)Session["Discount"] : 0;

            // Tính tiền giảm
            decimal tienGiam = Math.Round(tamTinh * (discountPercent / 100), 0);

            // Tính tổng sau giảm
            decimal tongSauGiam = tamTinh - tienGiam;

            // Gửi sang View
            ViewBag.TamTinh = tamTinh;
            ViewBag.DiscountPercent = discountPercent;
            ViewBag.TienGiam = tienGiam;
            ViewBag.TongSauGiam = tongSauGiam;

            return View(cart);
        }

        [HttpPost]
        public ActionResult ThemVaoGio(int maCTSP, int? soLuong)
        {
            try
            {
                int qty = soLuong.GetValueOrDefault(1); // nếu null → 1

                var ctsp = db.CHITIET_SANPHAM
                             .Include("SANPHAM")
                             .FirstOrDefault(x => x.MaCTSP == maCTSP);

                if (ctsp == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });

                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
                var item = cart.FirstOrDefault(x => x.MaCTSP == maCTSP);

                if (item == null)
                {
                    string hinh = db.HINHANHs
                                    .Where(h => h.MaCTSP == maCTSP)
                                    .Select(h => h.DuongDan)
                                    .FirstOrDefault();

                    cart.Add(new CartItem
                    {
                        MaCTSP = ctsp.MaCTSP,
                        TenSP = ctsp.SANPHAM.TenSP,
                        TenBienThe = ctsp.TenBienThe,
                        Gia = (decimal)(ctsp.GiaKhuyenMai ?? ctsp.Gia),
                        SoLuong = qty,
                        HinhAnh = hinh
                    });
                }
                else
                {
                    item.SoLuong += qty;
                }

                Session["Cart"] = cart;

                return Json(new
                {
                    success = true,
                    count = cart.Sum(x => x.SoLuong),
                    tongTien = cart.Sum(x => x.ThanhTien).ToString("N0") + "₫"
                });
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi thêm giỏ hàng!" });
            }
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult XoaKhoiGio(int maCTSP)
        {
            var cart = LayGioHang();
            var item = cart.FirstOrDefault(x => x.MaCTSP == maCTSP);
            if (item != null) cart.Remove(item);
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        public ActionResult CapNhat(int maCTSP, int soLuong)
        {
            var cart = LayGioHang();
            var item = cart.FirstOrDefault(x => x.MaCTSP == maCTSP);
            if (item != null) item.SoLuong = soLuong;
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public ActionResult XoaTatCa()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult KiemTraMaGiam(string ma)
        {
            if (string.IsNullOrEmpty(ma))
                return Json(new { success = false, message = "Vui lòng nhập mã!" });

            var km = db.KHUYENMAIs.FirstOrDefault(x =>
                x.TenKhuyenMai == ma && x.TrangThai == true);

            if (km == null)
                return Json(new { success = false, message = "Mã giảm giá không tồn tại!" });

            if (km.NgayBatDau > DateTime.Now || km.NgayKetThuc < DateTime.Now)
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn!" });

            // Lưu % giảm vào session
            decimal phanTram = (decimal)km.PhanTramGiam;
            Session["Discount"] = phanTram;

            // Lấy giỏ hàng
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || cart.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng trống!" });

            decimal tamTinh = cart.Sum(x => x.ThanhTien);
            decimal giam = Math.Round(tamTinh * (phanTram / 100), 0);
            decimal tong = tamTinh - giam;

            return Json(new
            {
                success = true,
                phanTram = phanTram,
                tamTinh = tamTinh,
                giam = giam,
                tong = tong
            });
        }


    }
}