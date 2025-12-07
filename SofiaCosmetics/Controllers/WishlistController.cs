using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.ViewModels;

namespace SofiaCosmetics.Controllers
{
    public class WishlistController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();
        // GET: Wishlist
        private List<WishlistItem> LayWishlist()
        {
            var wishlist = Session["Wishlist"] as List<WishlistItem>;
            if (wishlist == null)
            {
                wishlist = new List<WishlistItem>();
                Session["Wishlist"] = wishlist;
            }
            return wishlist;
        }

        // Xem danh sách yêu thích
        public ActionResult Index()
        {
            var wishlist = LayWishlist();
            return View(wishlist);
        }

        // Thêm sản phẩm vào danh sách yêu thích
        public ActionResult ThemYeuThich(int id)
        {
            var sp = db.SANPHAMs
                .Include("CHITIET_SANPHAM.HINHANHs")
                .FirstOrDefault(x => x.MaSP == id);

            if (sp == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" },
                            JsonRequestBehavior.AllowGet);

            var wishlist = LayWishlist();

            // Nếu chưa có thì thêm vào
            if (!wishlist.Any(x => x.MaSP == id))
            {
                var hinh = sp.CHITIET_SANPHAM
                             .SelectMany(ct => ct.HINHANHs)
                             .Select(h => h.DuongDan)
                             .FirstOrDefault();

                wishlist.Add(new WishlistItem
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.CHITIET_SANPHAM.FirstOrDefault()?.Gia ?? 0,
                    HinhAnh = hinh ?? "/images/products/no-image.png"
                });

                Session["Wishlist"] = wishlist;
                return Json(new { success = true, count = wishlist.Count }, JsonRequestBehavior.AllowGet);
            }

            // Nếu đã tồn tại
            return Json(new { success = false, message = "Đã có trong danh sách yêu thích" },
                        JsonRequestBehavior.AllowGet);
        }

        // Xóa sản phẩm khỏi yêu thích
        public ActionResult Xoa(int id)
        {
            var wishlist = LayWishlist();
            var item = wishlist.FirstOrDefault(x => x.MaSP == id);
            if (item != null) wishlist.Remove(item);
            Session["Wishlist"] = wishlist;
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ
        public ActionResult XoaTatCa()
        {
            Session["Wishlist"] = null;
            return RedirectToAction("Index");
        }
    }
}