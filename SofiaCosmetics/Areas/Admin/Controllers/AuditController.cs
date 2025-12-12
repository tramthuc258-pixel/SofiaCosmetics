using SofiaCosmetics.Areas.Admin.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class AuditController : BaseAdminController
    {
        // view trang log
        public ActionResult Index()
        {
            return View();
        }

        // lấy log JSON cho modal / page
        public JsonResult GetLogs(int take = 300)
        {
            try
            {
                var path = Server.MapPath("~/App_Data/audit.log");
                if (!System.IO.File.Exists(path))
                    return Json(new List<string>(), JsonRequestBehavior.AllowGet);

                var lines = System.IO.File.ReadAllLines(path, Encoding.UTF8)
                                          .Reverse()
                                          .Take(take)
                                          .ToList();

                return Json(lines, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
