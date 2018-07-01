using NodaTime;
using NodaTime.TimeZones;
using LazyCache;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace MagicMaids
{
	public static class CultureHelper
	{
		private static string DEFAULT_CULTURE = "au";
		private static int DEFAULT_LCID = 3081;
		private static string DEFAULT_TIMEZONE = "Australia/Perth";

		internal static string DisplayCultureSettings(string seperator = "\n")
		{
			StringBuilder output = new StringBuilder();

			DateTime _serverDateTime = DateTime.Now;
			Instant _nodeDateTime = DateTimeWrapper.NowInstance;

			output.Append($"FROM SERVER LOCATION:{seperator}");
			output.Append($"Current server DateTime (DateTime.Now): {_serverDateTime.ToString()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"UTC:{seperator}");
			output.Append($"Current NodeTime UTC (DateTimeWrapper.Now): {_nodeDateTime.ToString()}{seperator}");
			output.Append($"UTC for user's current date time (DateTimeWrapper.ToUTC: {_serverDateTime.ToUTC()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"FROM USER LOCATION (BROWSER):{seperator}");
			output.Append($"Current time at user location (DateTimeWrapper.DisplayLocalNow Method): {DateTimeWrapper.DisplayLocalNow()}{seperator}");
			output.Append($"Current time at user location (DateTimeWrapper.ToUser Extension): {_nodeDateTime.ToDateTimeUtc().ToUser()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"DATE ONLY - USER LOCATION:{seperator}");
			output.Append($"Current date at user location (DateTimeWrapper.ToUserDate Method): {DateTimeWrapper.NowUtc.ToUser().Date}{seperator}");
			output.Append($"Current date at UTC location (DateTimeWrapper.ToUtcDate Extension): {_nodeDateTime.ToDateTimeUtc().ToUTC().Date}{seperator}");

			output.Append($"{seperator}");

			output.Append($"FROM BROWSER:{seperator}");
			output.Append($"User Country Code: {UserCountryCode()}{seperator}");
			output.Append($"Default Time Zone: {DateTimeZoneProviders.Tzdb.GetSystemDefault().ToString()}{seperator}");
			output.Append($"User Timezone Name (Browser): {UserTimeZoneName()}{seperator}");
			output.Append($"User Timezone (Browser): {GetDateTimeZone().ToString()}{seperator}");
			output.Append($"User Timezone Offset in minutes: {UserTimeZoneOffsetMins().ToString()}{seperator}");
			output.Append($"{seperator}");
			output.Append($"User Timezones (from Country Code):{seperator}{LogHelper.GetObjectData(GetTimeZonesByCountry())}{seperator}");
			output.Append($"{seperator}");

			//output.Append($"{seperator}");
			//output.Append($"DEBUG:{seperator}");
			//var _debug = _debugDetails.Replace("|", $"{seperator}");
			//output.Append($"{_debug}");
			//output.Append($"{seperator}");

			return output.ToString();
		}

		// TimeZone from browser 
		internal static DateTimeZone GetDateTimeZone()
		{
			String countryCode = UserCountryCode();

			IAppCache cache = new CachingService();
			DateTimeZone TimeZone = cache.GetOrAdd($"dateTimeZone-{countryCode}", () => GetDateTimeZonePrivate(), new TimeSpan(0, 30, 0));

			if (TimeZone == null)
			{
				// set default time zone
				TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			}

			return TimeZone;
		}

		/// <summary>
		/// Gets and sets the time zone name in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A two letter country code</returns>
		public static string UserTimeZoneName()
		{
			const string sessionKeyName = "timezonename";
		    if (HttpContext.Current == null)
			{
				return DEFAULT_TIMEZONE;
			}

			string timeZoneName = (string)HttpContext.Current.Session[sessionKeyName];
			string countryCode = UserCountryCode();

			if (String.IsNullOrWhiteSpace(timeZoneName) || String.IsNullOrWhiteSpace(countryCode))
			{
				HttpContext.Current.Session.Remove(sessionKeyName);
				return DEFAULT_TIMEZONE;
			}

			if (timeZoneName.Contains("%2F"))
			{
				timeZoneName = timeZoneName.Replace("%2F", "/");
				HttpContext.Current.Session[sessionKeyName] = timeZoneName;
			}

			if (!GetTimeZonesByCountryPrivate(countryCode).Contains(timeZoneName))
			{
				HttpContext.Current.Session.Remove(sessionKeyName);
				return DEFAULT_TIMEZONE;
			}

			return timeZoneName;
		}


		private static DateTimeZone GetDateTimeZonePrivate()
		{
			DateTimeZone _userTZ = null;
			var _zone = UserTimeZoneName();

			if (!String.IsNullOrWhiteSpace(_zone) && DateTimeZoneProviders.Tzdb.Ids.Contains(_zone))
			{
				_userTZ = DateTimeZoneProviders.Tzdb.GetZoneOrNull(_zone);
			}

			return _userTZ;
		}

		/// <summary>
		/// Gets and sets the time zone offset in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A numeric offset from UTC</returns>
		internal static Int32 UserTimeZoneOffsetMins()
		{
			const string sessionKeyName = "timezoneoffset";
			string timezoneOffset = "0";

			timezoneOffset = (string)HttpContext.Current.Session[sessionKeyName];
			Int32 offset = 0;
			if (Int32.TryParse(timezoneOffset, out offset))
			{
				return offset;
			}

			return 0;
		}



		//https://codeshare.co.uk/blog/how-to-show-utc-time-relative-to-the-users-local-time-on-a-net-website/
		private static IEnumerable<String> GetTimeZonesByCountry()
		{
			String countryCode = UserCountryCode();

			IAppCache cache = new CachingService();
			IEnumerable<String> zoneIds = cache.GetOrAdd($"dateTimeZoneList-{countryCode}", () => GetTimeZonesByCountryPrivate(countryCode), new TimeSpan(0, 30, 0));
			if (zoneIds == null || zoneIds.Count() == 0)
			{
				cache.Remove($"dateTimeZoneList-{countryCode}");
				return new List<String>();
			}

			return zoneIds.ToList();
		}


		private static string UserCountryCode()
		{
			const string sessionKeyName = "UserCountryCode";
			string countryCode = "";

			if (HttpContext.Current == null)
			{
				return "";	
			}

			if (HttpContext.Current.Session[sessionKeyName] == null)
			{
				CultureInfo culture = ResolveCulture();
				var cultNum = (culture != null && culture.LCID != 127 && !culture.IsNeutralCulture) ? culture.LCID : DEFAULT_LCID;
				var regInfo = new RegionInfo(cultNum);
				countryCode = regInfo.ToString();
				HttpContext.Current.Session[sessionKeyName] = countryCode;
			}
			else
			{
				countryCode = (string)HttpContext.Current.Session[sessionKeyName];
			}

			return countryCode;
		}

		private static CultureInfo ResolveCulture()
		{
			string[] languages = HttpContext.Current.Request.UserLanguages;

			if (languages == null || languages.Length == 0)
			{
				return CultureInfo.CreateSpecificCulture(DEFAULT_CULTURE);
			}

			try
			{
				string language = languages[0].ToLowerInvariant().Trim();
				return CultureInfo.CreateSpecificCulture(language);
			}
			catch (ArgumentException)
			{
				return CultureInfo.CreateSpecificCulture(DEFAULT_CULTURE);
			}
		}

		private static IEnumerable<String> GetTimeZonesByCountryPrivate(String countryCode)
		{
			if (String.IsNullOrWhiteSpace(countryCode))
			{
				return new List<String>();
			}

			IEnumerable<String> zones = TzdbDateTimeZoneSource.Default.ZoneLocations
							.Where(x => x.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
							.Select(x => x.ZoneId);

			return zones.ToList();
		}
	}
}
