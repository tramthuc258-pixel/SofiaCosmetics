using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Filters
{
    public class AdminAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var path = filterContext.HttpContext.Request.Path.ToLower();

            if (path.Contains("/admin/dangnhap/login") || path.Contains("/admin/dangnhap/logout"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (filterContext.HttpContext.Session["ADMIN_LOGIN"] == null)
            {
                filterContext.Result = new RedirectResult("/Admin/DangNhap/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}