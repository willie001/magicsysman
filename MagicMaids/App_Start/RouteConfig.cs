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
			routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Errors",
                url: "Pages/Error/{errorCode}/{ex}",

                defaults: new { controller = "Pages", action = "Error", errorCode = UrlParameter.Optional, ex = UrlParameter.Optional }
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
