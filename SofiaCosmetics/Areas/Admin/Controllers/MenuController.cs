using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using SofiaCosmetics.Areas.Admin.Helpers;   // ✅ thêm dòng này
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class MenuController : BaseAdminController
    {
        // ================================
        // DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        // ================================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 5)
        {
            var list = db.MENUs
                .OrderBy(x => x.Id)
                .ToList()
                .Select(m => new AdminMenu
                {
                    Id = m.Id,
                    MenuName = m.MenuName,
                    MenuLink = m.MenuLink,
                    ParentId = m.ParentId,
                    ParentName = m.ParentId == null
                        ? "—"
                        : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault()
                }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();

                list = list.Where(x =>
                    x.MenuName.ToLower().Contains(kw) ||
                    (x.ParentName != null && x.ParentName.ToLower().Contains(kw)) ||
                    x.MenuLink.ToLower().Contains(kw) ||
                    ("md" + x.Id.ToString("000")).Contains(kw)
                ).ToList();
            }

            ViewBag.Search = search;

            int totalPage = (int)Math.Ceiling((double)list.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.MenuParent = db.MENUs.OrderBy(x => x.MenuName).ToList();

            return View(list);
        }

        // =============== THÊM MENU ===============
        [HttpPost]
        public JsonResult Add(AddMenuModel model)
        {
            try
            {
                MENU m = new MENU
                {
                    MenuName = model.MenuName,
                    MenuLink = model.MenuLink,
                    ParentId = model.ParentId,
                    OrderNumber = 1
                };

                db.MENUs.Add(m);
                db.SaveChanges();

                // ✅ LOG
                string parentName = (m.ParentId == null)
                    ? "—"
                    : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault();

                AuditLogger.Log(
                    module: "Menu",
                    action: "CREATE",
                    target: $"MENU#{m.Id}",
                    note: $"MenuName={m.MenuName}, Link={m.MenuLink}, Parent={parentName}"
                );

                return Json(true);
            }
            catch (Exception ex)
            {
                // (tuỳ bạn) log lỗi nếu muốn, còn không thì bỏ
                AuditLogger.Log("Menu", "ERROR_CREATE", "MENU", ex.Message);
                return Json(false);
            }
        }

        // =============== GET MENU ===============
        public JsonResult Get(int id)
        {
            var m = db.MENUs.Find(id);
            if (m == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                m.Id,
                m.MenuName,
                m.MenuLink,
                m.ParentId
            }, JsonRequestBehavior.AllowGet);
        }

        // =============== EDIT MENU ===============
        [HttpPost]
        public JsonResult Edit(EditMenuModel model)
        {
            try
            {
                var m = db.MENUs.Find(model.Id);
                if (m == null) return Json(false);

                // ✅ old info để log
                string oldParent = (m.ParentId == null)
                    ? "—"
                    : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault();

                string oldInfo =
                    $"MenuName={m.MenuName}, Link={m.MenuLink}, Parent={oldParent}";

                // update
                m.MenuName = model.MenuName;
                m.MenuLink = model.MenuLink;
                m.ParentId = model.ParentId;

                db.SaveChanges();

                string newParent = (m.ParentId == null)
                    ? "—"
                    : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault();

                string newInfo =
                    $"MenuName={m.MenuName}, Link={m.MenuLink}, Parent={newParent}";

                // ✅ LOG
                AuditLogger.Log(
                    module: "Menu",
                    action: "EDIT",
                    target: $"MENU#{m.Id}",
                    note: oldInfo + "  =>  " + newInfo
                );

                return Json(true);
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Menu", "ERROR_EDIT", $"MENU#{model?.Id}", ex.Message);
                return Json(false);
            }
        }

        // =============== XÓA MENU ===============
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var m = db.MENUs.Find(id);
                if (m == null) return Json(false);

                string parentName = (m.ParentId == null)
                    ? "—"
                    : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault();

                // ✅ LOG trước khi xóa
                AuditLogger.Log(
                    module: "Menu",
                    action: "DELETE",
                    target: $"MENU#{m.Id}",
                    note: $"MenuName={m.MenuName}, Link={m.MenuLink}, Parent={parentName}"
                );

                db.MENUs.Remove(m);
                db.SaveChanges();

                return Json(true);
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Menu", "ERROR_DELETE", $"MENU#{id}", ex.Message);
                return Json(false);
            }
        }

        // =============== XEM CHI TIẾT ===============
        public JsonResult Details(int id)
        {
            var m = db.MENUs.Find(id);
            if (m == null) return Json(null, JsonRequestBehavior.AllowGet);

            string parent = m.ParentId == null
                ? "—"
                : db.MENUs.Where(x => x.Id == m.ParentId).Select(x => x.MenuName).FirstOrDefault();

            // (optional) ✅ nếu bạn muốn log cả việc xem chi tiết thì mở dòng này
            // AuditLogger.Log("Menu", "VIEW", $"MENU#{m.Id}", $"MenuName={m.MenuName}");

            return Json(new
            {
                m.MenuName,
                m.MenuLink,
                Parent = parent
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
