using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class KhuyenMaiController : BaseAdminController
    {
        //QLMyPhamEntities db = new QLMyPhamEntities();

        // ===========================
        // DANH SÁCH + TÌM KIẾM
        // ===========================
        public ActionResult Index(string search = "", string status = "all", int page = 1, int pageSize = 2)
        {
            var list = db.KHUYENMAIs
                .OrderByDescending(x => x.MaKM)
                .ToList()
                .Select(x =>
                {
                    bool hetHan = x.NgayKetThuc.HasValue && DateTime.Now.Date > x.NgayKetThuc.Value.Date;

                    return new AdminKhuyenMai
                    {
                        MaKM = x.MaKM,
                        TenKhuyenMai = x.TenKhuyenMai,
                        MoTa = x.MoTa,
                        PhanTramGiam = x.PhanTramGiam,
                        NgayBatDau = x.NgayBatDau,
                        NgayKetThuc = x.NgayKetThuc,
                        TrangThai = x.TrangThai == true,
                        HetHan = hetHan
                    };
                }).ToList();

            // ===== Search như bạn đang làm =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                string kwNoMark = KhachHangController.RemoveUnicode(kw);

                list = list.Where(x =>
                    ("km" + x.MaKM.ToString("000")).ToLower().Contains(kw) ||
                    x.MaKM.ToString().Contains(kw) ||
                    (x.TenKhuyenMai != null && KhachHangController.RemoveUnicode(x.TenKhuyenMai.ToLower()).Contains(kwNoMark)) ||
                    (x.MoTa != null && KhachHangController.RemoveUnicode(x.MoTa.ToLower()).Contains(kwNoMark))
                ).ToList();
            }

            // ===== Filter trạng thái =====
            switch ((status ?? "all").ToLower())
            {
                case "active":   // Hoạt động (TrangThai=1 và chưa hết hạn)
                    list = list.Where(x => x.TrangThai && !x.HetHan).ToList();
                    break;

                case "expired":  // Hết hạn (quá ngày kết thúc)
                    list = list.Where(x => x.HetHan).ToList();
                    break;

                case "inactive": // Ngừng (TrangThai=0 và chưa hết hạn)
                    list = list.Where(x => !x.TrangThai && !x.HetHan).ToList();
                    break;

                default:
                    break;
            }

            ViewBag.Search = search;
            ViewBag.Status = status;

            int totalPage = (int)Math.Ceiling((double)list.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View(list);
        }

        // ===========================
        // THÊM KHUYẾN MÃI
        // ===========================
        [HttpPost]
        public JsonResult Add(AddKhuyenMaiModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenKhuyenMai))
                    return Json(false);

                if (model.PhanTramGiam < 0 || model.PhanTramGiam > 100)
                    return Json(false);

                if (model.NgayBatDau > model.NgayKetThuc)
                    return Json(false);

                KHUYENMAI km = new KHUYENMAI
                {
                    TenKhuyenMai = model.TenKhuyenMai,
                    MoTa = model.MoTa,
                    PhanTramGiam = model.PhanTramGiam,
                    NgayBatDau = model.NgayBatDau,
                    NgayKetThuc = model.NgayKetThuc,
                    TrangThai = model.TrangThai
                };

                db.KHUYENMAIs.Add(km);
                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ===========================
        // GET THÔNG TIN ĐỂ SỬA
        // ===========================
        public JsonResult Get(int id)
        {
            var km = db.KHUYENMAIs.Find(id);
            if (km == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                km.MaKM,
                km.TenKhuyenMai,
                km.MoTa,
                km.PhanTramGiam,
                NgayBatDau = km.NgayBatDau?.ToString("yyyy-MM-dd"),
                NgayKetThuc = km.NgayKetThuc?.ToString("yyyy-MM-dd"),
                km.TrangThai
            }, JsonRequestBehavior.AllowGet);
        }

        // ===========================
        // SỬA KHUYẾN MÃI
        // ===========================
        [HttpPost]
        public JsonResult Edit(EditKhuyenMaiModel model)
        {
            try
            {
                var km = db.KHUYENMAIs.Find(model.MaKM);
                if (km == null) return Json(false);

                if (string.IsNullOrWhiteSpace(model.TenKhuyenMai))
                    return Json(false);

                if (model.PhanTramGiam < 0 || model.PhanTramGiam > 100)
                    return Json(false);

                if (model.NgayBatDau > model.NgayKetThuc)
                    return Json(false);

                km.TenKhuyenMai = model.TenKhuyenMai;
                km.MoTa = model.MoTa;
                km.PhanTramGiam = model.PhanTramGiam;
                km.NgayBatDau = model.NgayBatDau;
                km.NgayKetThuc = model.NgayKetThuc;
                km.TrangThai = model.TrangThai;

                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ===========================
        // XÓA KHUYẾN MÃI (SOFT)
        // ===========================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var km = db.KHUYENMAIs.Find(id);
            if (km == null) return Json(false);

            // Gợi ý soft delete để tránh lỗi FK với SANPHAM
            km.TrangThai = false;
            db.SaveChanges();

            return Json(true);
        }

        // ===========================
        // XEM CHI TIẾT
        // ===========================
        public JsonResult Details(int id)
        {
            var km = db.KHUYENMAIs.Find(id);
            if (km == null) return Json(null, JsonRequestBehavior.AllowGet);

            bool hetHan = km.NgayKetThuc.HasValue && DateTime.Now.Date > km.NgayKetThuc.Value.Date;

            return Json(new
            {
                km.TenKhuyenMai,
                km.MoTa,
                km.PhanTramGiam,
                NgayBatDau = km.NgayBatDau?.ToString("dd/MM/yyyy"),
                NgayKetThuc = km.NgayKetThuc?.ToString("dd/MM/yyyy"),
                TrangThai = (km.TrangThai == true && !hetHan) ? "Hoạt động" : (hetHan ? "Hết hạn" : "Ngừng hoạt động")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}