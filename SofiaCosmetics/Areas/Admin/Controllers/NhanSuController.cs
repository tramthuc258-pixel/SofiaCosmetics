using SofiaCosmetics.Models;
using SofiaCosmetics.Areas.Admin.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class NhanSuController : BaseAdminController
    {
        // ====================== INDEX + SEARCH + PAGING ======================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 8)
        {
            string keyword = (search ?? "").Trim();
            string kwLower = keyword.ToLower();

            var q = db.ADMINs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(kwLower))
            {
                q = q.Where(x =>
                    (x.TenDangNhap ?? "").ToLower().Contains(kwLower) ||
                    (x.HoTen ?? "").ToLower().Contains(kwLower) ||
                    (x.Email ?? "").ToLower().Contains(kwLower) ||
                    (x.SDT ?? "").ToLower().Contains(kwLower) ||
                    (x.VaiTro ?? "").ToLower().Contains(kwLower)
                );
            }

            int totalItems = q.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var data = q.OrderByDescending(x => x.MaAdmin)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            ViewBag.Search = keyword;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(data);
        }

        // ====================== GET 1 NHÂN SỰ (JSON) ======================
        public JsonResult GetNhanSu(int id)
        {
            var ns = db.ADMINs.FirstOrDefault(x => x.MaAdmin == id);
            if (ns == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ns.MaAdmin,
                ns.TenDangNhap,
                ns.MatKhau,
                ns.HoTen,
                ns.Email,
                ns.SDT,
                ns.VaiTro,
                ns.TrangThai
            }, JsonRequestBehavior.AllowGet);
        }

        // ====================== CREATE ======================
        [HttpPost]
        public JsonResult Create(ADMIN model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenDangNhap))
                    return Json(new { success = false, message = "Thiếu username!" });

                if (string.IsNullOrWhiteSpace(model.MatKhau))
                    return Json(new { success = false, message = "Thiếu mật khẩu!" });

                bool existed = db.ADMINs.Any(x => x.TenDangNhap == model.TenDangNhap);
                if (existed)
                    return Json(new { success = false, message = "Username đã tồn tại!" });

                if (string.IsNullOrWhiteSpace(model.VaiTro))
                    model.VaiTro = "Nhân viên";

                // nếu null thì default true
                model.TrangThai = model.TrangThai ?? true;

                db.ADMINs.Add(model);
                db.SaveChanges();     // lúc này model.MaAdmin đã có

                // ===== LOG =====
                AuditLogger.Log(
                    module: "NhanSu",
                    action: "CREATE",
                    target: $"Admin#{model.MaAdmin}",
                    note: $"Username={model.TenDangNhap}, HoTen={model.HoTen}, VaiTro={model.VaiTro}, TrangThai={(model.TrangThai == true ? "Active" : "Inactive")}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi thêm nhân sự: " + ex.Message });
            }
        }

        // ====================== EDIT ======================
        [HttpPost]
        public JsonResult Edit(ADMIN model)
        {
            try
            {
                var ns = db.ADMINs.Find(model.MaAdmin);
                if (ns == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân sự!" });

                bool existed = db.ADMINs.Any(x => x.MaAdmin != model.MaAdmin && x.TenDangNhap == model.TenDangNhap);
                if (existed)
                    return Json(new { success = false, message = "Username bị trùng!" });

                // lưu trạng thái cũ để log
                string oldInfo =
                    $"Username={ns.TenDangNhap}, HoTen={ns.HoTen}, VaiTro={ns.VaiTro}, TrangThai={(ns.TrangThai == true ? "Active" : "Inactive")}";

                // update
                ns.TenDangNhap = model.TenDangNhap;
                ns.HoTen = model.HoTen;
                ns.Email = model.Email;
                ns.SDT = model.SDT;
                ns.VaiTro = model.VaiTro;
                ns.TrangThai = model.TrangThai;

                if (!string.IsNullOrWhiteSpace(model.MatKhau))
                    ns.MatKhau = model.MatKhau;

                db.SaveChanges();

                string newInfo =
                    $"Username={ns.TenDangNhap}, HoTen={ns.HoTen}, VaiTro={ns.VaiTro}, TrangThai={(ns.TrangThai == true ? "Active" : "Inactive")}";

                // ===== LOG =====
                AuditLogger.Log(
                    module: "NhanSu",
                    action: "EDIT",
                    target: $"Admin#{ns.MaAdmin}",
                    note: $"{oldInfo}  =>  {newInfo}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi sửa nhân sự: " + ex.Message });
            }
        }

        // ====================== DELETE ======================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var ns = db.ADMINs.Find(id);
                if (ns == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân sự!" });

                // không cho xóa quản lý
                string vaiTroLower = (ns.VaiTro ?? "").ToLower();
                if (vaiTroLower.Contains("quản lý") || vaiTroLower.Contains("quan ly"))
                    return Json(new { success = false, message = "Không được xóa tài khoản quản lý!" });

                string info =
                    $"Username={ns.TenDangNhap}, HoTen={ns.HoTen}, VaiTro={ns.VaiTro}, TrangThai={(ns.TrangThai == true ? "Active" : "Inactive")}";

                // ===== LOG trước khi xóa =====
                AuditLogger.Log(
                    module: "NhanSu",
                    action: "DELETE",
                    target: $"Admin#{ns.MaAdmin}",
                    note: info
                );

                db.ADMINs.Remove(ns);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xóa: " + ex.Message });
            }
        }

        // ====================== LẤY LOG NHÂN SỰ ======================
        // Dùng cho nút "Lịch sử thao tác" ở Index
        // ====================== LẤY LOG (tất cả module) ======================
        public JsonResult GetNhanSuLogs(int take = 200)
        {
            try
            {
                // ✅ chỉ cho tài khoản có VaiTro chứa "quản lý" xem log
                int maAdmin = (int)Session["ADMIN_LOGIN"];
                var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == maAdmin);
                string role = admin?.VaiTro ?? "";

                if (!role.ToLower().Contains("quản lý"))
                {
                    // nhân viên thường không thấy gì
                    return Json(new string[0], JsonRequestBehavior.AllowGet);
                }

                // ✅ đọc thẳng N dòng cuối, KHÔNG lọc theo module nữa
                var lines = AuditLogger.ReadLines(take);

                return Json(lines, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new string[0], JsonRequestBehavior.AllowGet);
            }
        }
    }
}
