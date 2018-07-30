using System;
using System.Net;
using System.Web.Mvc;

namespace MagicMaids
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class MagicAttribute: AuthorizeAttribute
	{
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (ConfigEnvironment.AllowAnonymous)
			{
				return;
			}

			base.OnAuthorization(filterContext);
			//if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
			//{
			//	filterContext.Result = new RedirectResult("~/User/Logon");
			//	return;
			//}

			//if (filterContext.HttpContext.User.IsInRole("Regular user"))
			//{
			//	filterContext.Result = new RedirectResult("~/Index/Subscribe");
			//}
		}
	}
}
