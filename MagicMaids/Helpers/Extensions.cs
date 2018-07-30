using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using MagicMaids.ViewModels;
using Newtonsoft.Json;

namespace MagicMaids
{
	public static class Extensions
	{
		public static string UserName(this ClaimsPrincipal user)
		{
			if (user == null)
			{
				return "guest";
			}

			var displayName = user.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
			var currentUser = displayName != null ? displayName.Value : string.Empty;

			if (String.IsNullOrWhiteSpace(currentUser))
				return "guest";

			return currentUser;
		}


		public static void StoreSearchCookieCiteria(this SearchVM criteria, string cookieName)
		{
			var cookie = "SearchCriteria";
			if (!String.IsNullOrWhiteSpace(cookieName))
			{
				cookie += $"_{cookieName}";
			}

			HttpCookie cookieCriteria = new HttpCookie(cookie)
			{
				Value = Uri.EscapeDataString(criteria.ToString())
			};

			cookieCriteria.Expires = DateTime.Now.AddMinutes(15);
			HttpContext.Current.Response.Cookies.Add(cookieCriteria);
		}


		public static SearchVM RestoreSearchCookieCriteria(this SearchVM criteria, string cookieName)
		{
			var cookie = "SearchCriteria";
			if (!String.IsNullOrWhiteSpace(cookieName))
			{
				cookie += $"_{cookieName}";
			}

			HttpCookie cookieCriteria = HttpContext.Current.Request.Cookies[cookie];

			if (cookieCriteria != null)
			{
				var cookieVal = cookieCriteria.Value;
				if (!String.IsNullOrWhiteSpace(cookieVal))
				{
					cookieVal = Uri.UnescapeDataString(cookieVal);
					criteria = JsonConvert.DeserializeObject<SearchVM>(cookieVal);
				}
			}

			return criteria;
		}
	}

}
