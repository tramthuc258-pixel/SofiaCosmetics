using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class ThuongHieuController : BaseAdminController
    {
        //QLMyPhamEntities db = new QLMyPhamEntities();

        // ============================
        // LIST + SEARCH + PAGING
        // ============================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 15)
        {
            var list = db.THUONGHIEUx
                .OrderByDescending(x => x.MaTH)
                .Select(x => new AdminThuongHieu
                {
                    MaTH = x.MaTH,
                    TenThuongHieu = x.TenThuongHieu,
                    MoTa = x.MoTa
                }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                list = list.Where(x =>
                    x.TenThuongHieu.ToLower().Contains(kw) ||
                    x.MoTa.ToLower().Contains(kw) ||
                    ("th" + x.MaTH.ToString("000")).ToLower().Contains(kw) ||
                    x.MaTH.ToString().Contains(kw)
                ).ToList();
            }

            ViewBag.Search = search;

            int totalPage = (int)Math.Ceiling((double)list.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View(list);
        }


        // ============================
        // ADD
        // ============================
        [HttpPost]
        public JsonResult Add(AddThuongHieuModel model)
        {
            try
            {
                var th = new THUONGHIEU
                {
                    TenThuongHieu = model.TenThuongHieu,
                    MoTa = model.MoTa
                };

                db.THUONGHIEUx.Add(th);
                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ============================
        // GET → EDIT/VIEW
        // ============================
        public JsonResult Get(int id)
        {
            var th = db.THUONGHIEUx.Find(id);
            if (th == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                th.MaTH,
                th.TenThuongHieu,
                th.MoTa
            }, JsonRequestBehavior.AllowGet);
        }

        // ============================
        // EDIT
        // ============================
        [HttpPost]
        public JsonResult Edit(EditThuongHieuModel model)
        {
            try
            {
                var th = db.THUONGHIEUx.Find(model.MaTH);
                if (th == null) return Json(false);

                th.TenThuongHieu = model.TenThuongHieu;
                th.MoTa = model.MoTa;

                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ============================
        // DELETE
        // ============================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var th = db.THUONGHIEUx.Find(id);
            if (th == null) return Json(false);

            db.THUONGHIEUx.Remove(th);
            db.SaveChanges();

            return Json(true);
        }


        // ============================
        // DETAILS
        // ============================
        public JsonResult Details(int id)
        {
            var th = db.THUONGHIEUx.Find(id);
            if (th == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                th.TenThuongHieu,
                th.MoTa
            }, JsonRequestBehavior.AllowGet);
        }
    }
}