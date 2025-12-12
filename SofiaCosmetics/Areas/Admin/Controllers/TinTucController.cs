using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using SofiaCosmetics.Areas.Admin.Helpers;   // ✅ thêm để dùng AuditLogger
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class TinTucController : BaseAdminController
    {
        // =====================
        // BỎ DẤU để search
        // =====================
        private string ToNoMark(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.ToLower().Trim();

            string formD = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char ch in formD)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("đ", "d");
        }

        // =====================
        // INDEX + SEARCH + PAGING
        // =====================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 7)
        {
            search = (search ?? "").Trim();
            string kw = search.ToLower();
            string kwNoMark = ToNoMark(kw);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 7;

            var q = db.TINTUCs.AsQueryable();

            // ===== 1) LỌC SQL TRƯỚC (không dùng ToNoMark trong SQL) =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                int num;
                bool isCodeSearch = false;

                string onlyNum = kw.Replace("tt", "");
                if (int.TryParse(onlyNum, out num))
                    isCodeSearch = true;

                q = q.Where(x =>
                    (isCodeSearch && x.MaTT == num)
                    || (x.TenTrang ?? "").Contains(search)
                    || (x.MetaTitle ?? "").Contains(search)
                );
            }

            var raw = q
                .OrderByDescending(x => x.MaTT)
                .Select(x => new AdminNewsListVM
                {
                    MaTT = x.MaTT,
                    TenTrang = x.TenTrang,
                    MetaTitle = x.MetaTitle,
                    NgayTao = x.NgayTao
                })
                .ToList();

            // ===== 2) LỌC BỔ SUNG IN-MEMORY =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                raw = raw.Where(x =>
                    ("tt" + x.MaTT.ToString("000")).ToLower().Contains(kw)
                    || x.MaTT.ToString().Contains(kw)
                    || ToNoMark((x.TenTrang ?? "").ToLower()).Contains(kwNoMark)
                    || ToNoMark((x.MetaTitle ?? "").ToLower()).Contains(kwNoMark)
                    || (x.TenTrang ?? "").ToLower().Contains(kw)
                    || (x.MetaTitle ?? "").ToLower().Contains(kw)
                ).ToList();
            }

            // ===== 3) PAGING =====
            int totalItem = raw.Count;
            int totalPage = (int)Math.Ceiling(totalItem / (double)pageSize);
            if (totalPage == 0) totalPage = 1;
            if (page > totalPage) page = totalPage;

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            var data = raw.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(data);
        }

        // =====================
        // DETAILS JSON (modal xem/sửa)
        // =====================
        [HttpGet]
        public JsonResult Details(int id)
        {
            var tt = db.TINTUCs.Find(id);
            if (tt == null)
                return Json(new { success = false, message = "Không tìm thấy bài viết!" }, JsonRequestBehavior.AllowGet);

            // (optional) ✅ nếu bạn muốn log hành vi xem chi tiết thì mở comment này
            // AuditLogger.Log("TinTuc", "VIEW", $"TT#{tt.MaTT}", $"TenTrang={tt.TenTrang}, MetaTitle={tt.MetaTitle}");

            return Json(new
            {
                success = true,
                data = new
                {
                    tt.MaTT,
                    MaTTText = "TT" + tt.MaTT.ToString("000"),
                    TenTrang = tt.TenTrang ?? "",
                    MetaTitle = tt.MetaTitle ?? "",
                    NoiDung = tt.NoiDung ?? "",
                    NgayTao = tt.NgayTao?.ToString("dd/MM/yyyy HH:mm") ?? ""
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // =====================
        // CREATE
        // =====================
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult CreateAjax(AdminNewsFormVM model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Thiếu dữ liệu!" });

            try
            {
                string meta = (model.MetaTitle ?? "").Trim().ToLower();
                bool existed = db.TINTUCs.Any(x => x.MetaTitle.ToLower() == meta);
                if (existed)
                    return Json(new { success = false, message = "Meta title đã tồn tại!" });

                var entity = new TINTUC
                {
                    TenTrang = (model.TenTrang ?? "").Trim(),
                    MetaTitle = meta,
                    NoiDung = model.NoiDung ?? "",
                    NgayTao = DateTime.Now
                };

                db.TINTUCs.Add(entity);
                db.SaveChanges();

                // ✅ LOG
                AuditLogger.Log(
                    module: "TinTuc",
                    action: "CREATE",
                    target: $"TT#{entity.MaTT}",
                    note: $"TenTrang={entity.TenTrang}, MetaTitle={entity.MetaTitle}"
                );

                return Json(new { success = true, message = "Đã thêm bài viết!" });
            }
            catch (Exception ex)
            {
                // (tuỳ bạn) log lỗi
                AuditLogger.Log("TinTuc", "ERROR_CREATE", "TT", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =====================
        // EDIT
        // =====================
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EditAjax(AdminNewsFormVM model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Thiếu dữ liệu!" });

            try
            {
                var old = db.TINTUCs.Find(model.MaTT);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy bài viết!" });

                string meta = (model.MetaTitle ?? "").Trim().ToLower();
                bool existed = db.TINTUCs.Any(x => x.MaTT != model.MaTT && x.MetaTitle.ToLower() == meta);
                if (existed)
                    return Json(new { success = false, message = "Meta title bị trùng!" });

                // ✅ old info để log
                string oldInfo = $"TenTrang={old.TenTrang}, MetaTitle={old.MetaTitle}";

                old.TenTrang = (model.TenTrang ?? "").Trim();
                old.MetaTitle = meta;
                old.NoiDung = model.NoiDung ?? "";

                db.SaveChanges();

                string newInfo = $"TenTrang={old.TenTrang}, MetaTitle={old.MetaTitle}";

                // ✅ LOG
                AuditLogger.Log(
                    module: "TinTuc",
                    action: "EDIT",
                    target: $"TT#{old.MaTT}",
                    note: oldInfo + "  =>  " + newInfo
                );

                return Json(new { success = true, message = "Đã cập nhật bài viết!" });
            }
            catch (Exception ex)
            {
                AuditLogger.Log("TinTuc", "ERROR_EDIT", $"TT#{model?.MaTT}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =====================
        // DELETE
        // =====================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var tt = db.TINTUCs.Find(id);
                if (tt == null)
                    return Json(new { success = false, message = "Không tìm thấy bài viết!" });

                // ✅ LOG trước khi xóa
                AuditLogger.Log(
                    module: "TinTuc",
                    action: "DELETE",
                    target: $"TT#{tt.MaTT}",
                    note: $"TenTrang={tt.TenTrang}, MetaTitle={tt.MetaTitle}"
                );

                db.TINTUCs.Remove(tt);
                db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa bài viết!" });
            }
            catch (Exception ex)
            {
                AuditLogger.Log("TinTuc", "ERROR_DELETE", $"TT#{id}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
