using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class SliderController : BaseAdminController
    {

        // =========================
        // DANH SÁCH + TÌM KIẾM + FILTER
        // =========================
        public ActionResult Index(string search = "", string status = "all", int page = 1, int pageSize = 3)
        {
            var query = db.SLIDERs.AsQueryable();

            // =========================
            // SEARCH: theo mã SLxxx / số / tiêu đề
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                string kwRaw = search.Trim().ToLower();

                // nếu user gõ "sl001" hay "SL001" -> lấy phần số
                string digits = new string(kwRaw.Where(char.IsDigit).ToArray());

                int idSearch = 0;
                bool isId = int.TryParse(digits, out idSearch);

                query = query.Where(x =>
                    // ✅ search theo mã slider
                    (isId && x.MaSlider == idSearch)

                    // ✅ search theo tiêu đề/mô tả/link
                    || (x.TieuDe ?? "").ToLower().Contains(kwRaw)
                    || (x.MoTa ?? "").ToLower().Contains(kwRaw)
                    || (x.Link ?? "").ToLower().Contains(kwRaw)
                );
            }

            // =========================
            // FILTER STATUS
            // =========================
            switch ((status ?? "all").ToLower())
            {
                case "active":
                    query = query.Where(x => x.TrangThai == true);
                    break;
                case "inactive":
                    query = query.Where(x => x.TrangThai == false);
                    break;
            }

            var list = query
                .OrderBy(x => x.ThuTu)
                .ThenByDescending(x => x.MaSlider)
                .ToList()
                .Select(x => new AdminSlider
                {
                    MaSlider = x.MaSlider,
                    TieuDe = x.TieuDe,
                    MoTa = x.MoTa,
                    HinhAnh = x.HinhAnh,
                    Link = x.Link,
                    ThuTu = x.ThuTu ?? 0,
                    TrangThai = x.TrangThai == true,
                    NgayTao = x.NgayTao
                }).ToList();

            ViewBag.Search = search;
            ViewBag.Status = status;

            int totalPage = (int)Math.Ceiling((double)list.Count / pageSize);
            if (totalPage == 0) totalPage = 1;
            if (page < 1) page = 1;
            if (page > totalPage) page = totalPage;

            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(list);
        }

        // =========================
        // ADD
        // =========================
        [HttpPost]
        public JsonResult Add(AddSliderModel model, HttpPostedFileBase ImageFile)
        {
            try
            {
                if (ImageFile == null || ImageFile.ContentLength == 0)
                    return Json(new { success = false, message = "Vui lòng chọn ảnh slider!" });

                // kiểm tra đuôi file
                var ext = Path.GetExtension(ImageFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                    return Json(new { success = false, message = "Chỉ hỗ trợ JPG/PNG/WEBP!" });

                // tạo tên file mới tránh trùng
                var fileName = "slider_" + DateTime.Now.Ticks + ext;
                var folder = Server.MapPath("~/Upload/images/slider/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var savePath = Path.Combine(folder, fileName);
                ImageFile.SaveAs(savePath);

                var imgUrl = "/Upload/images/slider/" + fileName;

                SLIDER s = new SLIDER
                {
                    TieuDe = model.TieuDe,
                    MoTa = model.MoTa,
                    Link = model.Link,
                    HinhAnh = imgUrl,
                    ThuTu = model.ThuTu,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };

                db.SLIDERs.Add(s);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi thêm slider: " + ex.Message });
            }
        }

        // =========================
        // GET EDIT
        // =========================
        public JsonResult Get(int id)
        {
            var s = db.SLIDERs.Find(id);
            if (s == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                s.MaSlider,
                s.TieuDe,
                s.MoTa,
                s.HinhAnh,
                s.Link,
                ThuTu = s.ThuTu ?? 0,
                s.TrangThai
            }, JsonRequestBehavior.AllowGet);
        }

        // =========================
        // EDIT
        // =========================
        [HttpPost]
        public JsonResult Edit(EditSliderModel model, HttpPostedFileBase ImageFile, string OldImage)
        {
            try
            {
                var s = db.SLIDERs.Find(model.MaSlider);
                if (s == null)
                    return Json(new { success = false, message = "Không tìm thấy slider!" });

                string imgUrl = OldImage;

                // nếu có chọn ảnh mới -> upload đè
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var ext = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                        return Json(new { success = false, message = "Chỉ hỗ trợ JPG/PNG/WEBP!" });

                    var fileName = "slider_" + DateTime.Now.Ticks + ext;
                    var folder = Server.MapPath("~/Upload/images/slider/");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var savePath = Path.Combine(folder, fileName);
                    ImageFile.SaveAs(savePath);
                    imgUrl = "/Upload/images/slider/" + fileName;

                    // xóa ảnh cũ nếu muốn (không bắt buộc)
                    try
                    {
                        if (!string.IsNullOrEmpty(OldImage))
                        {
                            var oldPath = Server.MapPath(OldImage);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                    }
                    catch { }
                }

                s.TieuDe = model.TieuDe;
                s.MoTa = model.MoTa;
                s.Link = model.Link;
                s.HinhAnh = imgUrl;
                s.ThuTu = model.ThuTu;
                s.TrangThai = model.TrangThai;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi sửa slider: " + ex.Message });
            }
        }

        // =========================
        // DELETE (SOFT)
        // =========================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var s = db.SLIDERs.Find(id);
                if (s == null) return Json(new { success = false, message = "Không tìm thấy slider!" });

                // ✅ Xóa file ảnh vật lý (nếu có)
                if (!string.IsNullOrEmpty(s.HinhAnh))
                {
                    try
                    {
                        var oldPath = Server.MapPath(s.HinhAnh);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    catch { /* không bắt buộc */ }
                }

                // ✅ XÓA CỨNG DB
                db.SLIDERs.Remove(s);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xóa slider: " + ex.Message });
            }
        }

        // =========================
        // DETAILS
        // =========================
        public JsonResult Details(int id)
        {
            var s = db.SLIDERs.Find(id);
            if (s == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                s.TieuDe,
                s.MoTa,
                s.HinhAnh,
                s.Link,
                ThuTu = s.ThuTu ?? 0,
                TrangThaiText = s.TrangThai == true ? "Hiển thị" : "Ẩn",
                NgayTao = s.NgayTao?.ToString("dd/MM/yyyy HH:mm")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
