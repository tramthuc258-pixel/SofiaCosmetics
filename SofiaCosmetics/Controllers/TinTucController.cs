using SofiaCosmetics.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Controllers
{
    public class TinTucController : Controller
    {
        private readonly QLMyPhamEntities db = new QLMyPhamEntities();

        // /tin-tuc  => danh sách tin (lọc blog)
        public ActionResult Index()
        {
            var list = db.TINTUCs
                //.Where(x => x.IsBlog == true)   // nếu chưa có cột IsBlog thì bỏ dòng này
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            return View("TinTuc", list);
        }

        // /tin-tuc/{slug} => chi tiết bài blog
        public ActionResult ChiTiet(string slug)
        {
            slug = (slug ?? "").Trim().ToLower();

            var model = db.TINTUCs.FirstOrDefault(x => x.MetaTitle == slug);
            if (model == null) return HttpNotFound();

            return View("TinTucChiTiet", model);
        }

        // /{slug} => các trang tĩnh
        public ActionResult Slug(string slug)
        {
            slug = (slug ?? "").Trim().ToLower();

            // lấy DB 
            var model = db.TINTUCs.FirstOrDefault(x => x.MetaTitle == slug);

            switch (slug)
            {
                case "gioi-thieu":
                    return View("GioiThieu", model);

                case "lien-he":
                    return View("LienHe", model);

                case "chinh-sach-giao-hang":
                    return View("ChinhSachGiaoHang", model);

                case "chinh-sach-doi-tra":
                    return View("ChinhSachDoiTra", model);

                case "chinh-sach-bao-mat":
                    return View("ChinhSachBaoMat", model);

                case "dieu-khoan-su-dung":
                    return View("DieuKhoanSuDung", model);

                case "huong-dan-mua-hang":
                    return View("HuongDanMuaHang", model);
                case "khuyen-mai":
                    return RedirectToAction("Index", "KhuyenMai");

                case "tin-tuc": return View("TinTuc", model);

                default:
                    // slug tự do (tin/bài viết) thì bắt buộc có dữ liệu
                    if (model == null) return HttpNotFound();
                    return View("Slug", model);
            }
        }
    }
}
