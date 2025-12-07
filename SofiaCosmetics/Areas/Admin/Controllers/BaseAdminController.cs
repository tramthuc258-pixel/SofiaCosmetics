using SofiaCosmetics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class BaseAdminController : Controller
    {
        protected QLMyPhamEntities db = new QLMyPhamEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Bỏ qua check login cho trang Login/Logout
            var path = filterContext.HttpContext.Request.Path.ToLower();
            if (path.Contains("/admin/dangnhap/login") || path.Contains("/admin/dangnhap/logout"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // chưa login thì đá về login
            if (Session["ADMIN_LOGIN"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { area = "Admin", controller = "DangNhap", action = "Login" }
                    )
                );
                return;
            }

            // lấy admin từ session và DB
            int maAdmin = (int)Session["ADMIN_LOGIN"];
            var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == maAdmin);

            // gán ViewBag chung cho toàn bộ view
            ViewBag.AdminInfo = admin;                 // CÁI NÀY Navbar ĐANG DÙNG
            ViewBag.ADMIN_NAME = admin?.HoTen ?? "admin";
            ViewBag.ADMIN_ROLE = admin?.VaiTro ?? "";

            base.OnActionExecuting(filterContext);
        }
    }
}