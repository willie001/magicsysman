using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Dapper;
using LazyCache;
using MagicMaids.DataAccess;
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

		/// <summary>
		/// Returns list of zones for a given suburb
		/// </summary>
		/// <returns>The zone list by suburb.</returns>
		/// <param name="suburbName">Suburb name.</param>
		public static List<string> GetZoneListBySuburb(this string suburbName, Boolean withDefaults)
		{
			if (String.IsNullOrWhiteSpace(suburbName))
			{
				return new List<string>();
			}

			IAppCache cache = new CachingService();
			List<string> zoneList = cache.GetOrAdd($"SuburbZones_{suburbName}", () => GetZoneList(suburbName), new TimeSpan(0, 20, 0));

			if (zoneList.Count == 0)
			{
				cache.Remove($"SuburbZones_{suburbName}");
				if (withDefaults)
				{
					zoneList = cache.GetOrAdd($"SuburbZones_Defaults", () => GetDefaultZoneList(), new TimeSpan(0, 20, 0));
				}
			}

			return zoneList;
		}

		private static List<string> GetZoneList(string suburbName)
		{
			if (String.IsNullOrWhiteSpace(suburbName))
			{
				return null;
			}

			List<String> _zoneList = new List<String>();
			using (DBManager db = new DBManager())
			{
				_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where SuburbName like '%{suburbName}%' or PostCode = '{suburbName}'").ToList();
			}


			if (_zoneList.Count == 0)
			{
				return _zoneList;
			}

			var _zoneCSV = String.Join(",", _zoneList);
			_zoneList = _zoneCSV.Split(new char[] { ',', ';' })
						  .Distinct()
						  .ToList();

			_zoneList.Sort();
			return _zoneList;
		}

		private static List<string> GetDefaultZoneList()
		{
			List<String> _zoneList = new List<String>();
			using (DBManager db = new DBManager())
			{
				_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where FranchiseId is not null").ToList();
			}

			if (_zoneList.Count == 0)
			{
				return _zoneList;
			}

			var _zoneCSV = String.Join(",", _zoneList);
			_zoneList = _zoneCSV.Split(new char[] { ',', ';' })
						  .Distinct()
						  .ToList();

			_zoneList.Sort();
			return _zoneList;
		}
	}

}
