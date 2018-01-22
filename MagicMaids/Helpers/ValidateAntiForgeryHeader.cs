using System;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MagicMaids
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class ValidateAntiForgeryHeader: FilterAttribute, IAuthorizationFilter
	{
		private const string KEY_NAME = "__RequestVerificationToken";

		public void OnAuthorization(AuthorizationContext filterContext)
		{
			try
			{
				string clientToken = filterContext.RequestContext.HttpContext.Request.Headers.Get(KEY_NAME);
				if (clientToken == null) throw new HttpAntiForgeryException(String.Format("Header does not contain {0}", KEY_NAME));

				string serverToken = filterContext.HttpContext.Request.Cookies.Get(KEY_NAME).Value;
				if (serverToken == null) throw new HttpAntiForgeryException(String.Format("Cookies does not contain {0}", KEY_NAME));

				AntiForgery.Validate(serverToken, clientToken);
			}
			catch (HttpAntiForgeryException)
			{
				throw new HttpAntiForgeryException("Anti forgery token cookie not found");
			}
		}
	}
}
