using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MagicMaids
{
    public class RouteConfig
    { 
        public static void RegisterRoutes(RouteCollection routes)
        {
			//https://www.exceptionnotfound.net/attribute-routing-vs-convention-routing/
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Error404",
				url: "pages/error404",
				defaults: new { controller = "pages", action = "Error", path = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "Error500",
				url: "pages/error500",
				defaults: new { controller = "Pages", action = "error", path = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "ServerVars",
				url: "settings/servervars",
				defaults: new { controller = "Settings", action = "ServerVars", path = UrlParameter.Optional }
			);


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "App", action = "Index", id = UrlParameter.Optional }
            );

			routes.AppendTrailingSlash = true;
        }
    }
}
