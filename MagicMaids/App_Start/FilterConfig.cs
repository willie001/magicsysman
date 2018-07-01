using System.Web.Mvc;

namespace MagicMaids
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
			filters.Add(new System.Web.Mvc.AuthorizeAttribute());
			//filters.Add(new RequireHttpsAttribute());
            //filters.Add(new HandleErrorAttribute());
			//filters.Add(new ValidateAntiForgeryHeader());
        }
    }
}
