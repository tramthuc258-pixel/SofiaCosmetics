using System.Web.Mvc;
using System.Web.Routing;

namespace SofiaCosmetics.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var adminRoute = context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "SofiaCosmetics.Areas.Admin.Controllers" }
            );

            // khóa namespace để MVC không mò qua Controllers ngoài Area
            adminRoute.DataTokens = adminRoute.DataTokens ?? new RouteValueDictionary();
            adminRoute.DataTokens["Namespaces"] = new[] { "SofiaCosmetics.Areas.Admin.Controllers" };
            adminRoute.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}
