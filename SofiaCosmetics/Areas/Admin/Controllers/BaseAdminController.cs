using SofiaCosmetics.Models;
using SofiaCosmetics.Areas.Admin.Helpers;
using System.Linq;

using System.Web.Mvc;
using System.Web.Routing;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    // Nên để abstract để tránh bị gọi trực tiếp
    public abstract class BaseAdminController : Controller
    {
        protected QLMyPhamEntities db = new QLMyPhamEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var path = (filterContext.HttpContext.Request.Path ?? "").ToLower();

            // bỏ check login cho trang login/logout
            if (path.Contains("/admin/dangnhap/login") || path.Contains("/admin/dangnhap/logout"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // chưa login => về login
            if (Session["ADMIN_LOGIN"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { area = "Admin", controller = "DangNhap", action = "Login" })
                );
                return;
            }

            // lấy admin từ session ADMIN_LOGIN
            int maAdmin = (int)Session["ADMIN_LOGIN"];
            var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == maAdmin);

            if (admin == null)
            {
                // session lỗi/không tồn tại admin => logout
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { area = "Admin", controller = "DangNhap", action = "Logout" })
                );
                return;
            }

            // đẩy info cho navbar / layout
            ViewBag.AdminInfo = admin;
            ViewBag.ADMIN_NAME = admin.HoTen ?? admin.TenDangNhap ?? "admin";
            ViewBag.ADMIN_ROLE = admin.VaiTro ?? "";

            // ===== PHÂN QUYỀN =====
            string currentController =
                (filterContext.RouteData.Values["controller"]?.ToString() ?? "").Trim();

            var allowedModules =
                PermissionConfig.GetAllowedModules(admin.VaiTro, admin.TenDangNhap);

            ViewBag.AllowedModules = allowedModules;

            // nếu không có quyền => chặn
            if (!PermissionConfig.HasPermission(allowedModules, currentController))
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Areas/Admin/Views/Shared/NoPermission.cshtml"
                };
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
