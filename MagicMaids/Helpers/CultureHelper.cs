using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using LazyCache;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace MagicMaids
{
	public static class CultureHelper
	{
		//https://codeshare.co.uk/blog/how-to-show-utc-time-relative-to-the-users-local-time-on-a-net-website/
		private static string DEFAULT_CULTURE = "au";
		private static StringBuilder _debugDetails = new StringBuilder();
		private static Int32 _debugCount = 0;
		public static Boolean DateTimeInitialised = false;

		#region Methods, Public
		public static string FormatUserDate(this DateTime dt)
		{
			var newDate = dt.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
			return newDate;
		}

		public static string FormatUserDateTime(this DateTime dt)
		{
			var newDate = dt.ToString("d MMM yyyy HH:mm:ss");
			return newDate;
		}

		public static string FormatDatabaseDateTime(this DateTime dt)
		{
			var newDate = dt.ToString("yyyy-MM-dd HH:mm:ss:ff");
			return newDate;
		}

		public static string DisplayCultureSettings(string seperator = "\n")
		{
			StringBuilder output = new StringBuilder();

			DateTime _serverDateTime = DateTime.Now;
			Instant _nodeDateTime = CultureHelper.Now;

			output.Append($"FROM SERVER LOCATION:{seperator}");
			output.Append($"Current server DateTime (DateTime.Now): {_serverDateTime.ToString()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"UTC:{seperator}");
			output.Append($"Current NodeTime UTC (CultureHelper.Now): {_nodeDateTime.ToString()}{seperator}");
			output.Append($"UTC for user's current date time (CultureHelper.ToUTC: {_serverDateTime.ToUTC()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"FROM USER LOCATION (BROWSER):{seperator}");
			output.Append($"Current time at user location (CultureHelper.FormatLocalNow Method): {CultureHelper.FormatLocalNow()}{seperator}");
			output.Append($"Current time at user location (CultureHelper.ToUser Extension): {Now.ToDateTimeUtc().ToUser()}{seperator}");

			output.Append($"{seperator}");

			var regionInfo = GetDateTimeZoneFromCountryCode();
			var timeZoneName = regionInfo.ToString();

			output.Append($"FROM BROWSER:{seperator}");
			output.Append($"User Country Code: {CultureHelper.UserCountryCode()}{seperator}");
			output.Append($"User Timezone Name (Browser): {CultureHelper.UserTimeZoneName()}{seperator}");
			output.Append($"User Timezone Name (Country Code): {timeZoneName}{seperator}");
			output.Append($"User Timezone Offset in minutes: {CultureHelper.UserTimeZoneOffsetMins().ToString()}{seperator}");

			output.Append($"{seperator}");
			output.Append($"DEBUG:{seperator}");
			var _debug = _debugDetails.Replace("|", $"{seperator}");
			output.Append($"{_debug}");
			output.Append($"{seperator}");

			return output.ToString();
		}

		/// <summary>
		/// NodaTime's DateTime.Now
		/// </summary>
		/// <value>The now.</value>
		private static Instant Now
		{
			get
			{
				return SystemClock.Instance.GetCurrentInstant();
			}
		}

		public static string FormatLocalNow()
		{
			if (!DateTimeInitialised)
			{
				return "";		
			}
			else
			{
				return Now.ToDateTimeUtc().ToUser().FormatUserDateTime();
			}
		}

		/// <summary>
		/// Converts a non-local-time DateTime to a local-time DateTime based on the
		/// specified timezone. The returned object will be of Unspecified DateTimeKind 
		/// which represents local time agnostic to servers timezone. To be used when
		/// we want to convert UTC to local time somewhere in the world.
		/// </summary>
		/// <param name="dateTime">Non-local DateTime as UTC or Unspecified DateTimeKind.</param>
		/// <returns>Local DateTime as Unspecified DateTimeKind.</returns>
		public static DateTime ToUser(this DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				return dateTime;
			}
			else if (dateTime.Kind == DateTimeKind.Unspecified)
			{
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			}

			var zone = GetDateTimeZone();
			Instant instant = dateTime.ToInstant();
			ZonedDateTime inZone = instant.InZone(zone);
			DateTime unspecified = inZone.ToDateTimeUnspecified();

			return unspecified;
		}

		/// <summary>
		/// Converts a local-time DateTime to UTC DateTime based on the specified
		/// timezone. The returned object will be of UTC DateTimeKind. To be used
		/// when we want to know what's the UTC representation of the time somewhere
		/// in the world.
		/// </summary>
		/// <param name="dateTime">Local DateTime as UTC or Unspecified DateTimeKind.</param>
		/// <param name="timezone">Timezone name (in TZDB format).</param>
		/// <returns>UTC DateTime as UTC DateTimeKind.</returns>
		public static DateTime ToUTC(this DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				return dateTime.ToUniversalTime();
			}

			var zone = GetDateTimeZone();
			LocalDateTime asLocal = dateTime.ToLocalDateTime();
			ZonedDateTime asZoned = asLocal.InZoneLeniently(zone);
			Instant instant = asZoned.ToInstant();
			ZonedDateTime asZonedInUtc = instant.InUtc();
			DateTime utc = asZonedInUtc.ToDateTimeUtc();

			return utc;
		}
		#endregion 

		#region Methods, Private
		/// <summary>
		/// Gets and sets the time zone name in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A two letter country code</returns>
		private static string UserTimeZoneName()
		{
			if (!DateTimeInitialised)
				return "";

			const string sessionKeyName = "timezonename";
			string timeZoneName = (string)HttpContext.Current.Session[sessionKeyName];

			if (String.IsNullOrWhiteSpace(timeZoneName))
			{
				CultureHelper.DateTimeInitialised = false;
				return "";
			}

			if (timeZoneName.Contains("%2F"))
			{
				timeZoneName = timeZoneName.Replace("%2F", "/");
				HttpContext.Current.Session[sessionKeyName] = timeZoneName;
			}

			return timeZoneName;

		}

		/// <summary>
		/// Calls the method for getting the TimeZone from the country code, but sets the country code before calling it. 
		/// Helps to do this when using caching and the delegate method would have had parameters.
		/// </summary>
		/// <returns>A DateTimeZone from NodaTime based on the country code.</returns>
		private static DateTimeZone GetDateTimeZone()
		{
			IAppCache cache = new CachingService();
			DateTimeZone TimeZone = cache.GetOrAdd($"dateTimeZone-{UserCountryCode()}", () => GetDateTimeZonePrivate(), new TimeSpan(0, 30, 0));
			return TimeZone;
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
		/// Gets and sets the country code in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A two letter country code</returns>
		private static string UserCountryCode()
		{
			const string sessionKeyName = "UserCountryCode";
			string countryCode = "";

			if (HttpContext.Current.Session[sessionKeyName] == null)
			{
				countryCode = ResolveCountry().ToString();
				HttpContext.Current.Session[sessionKeyName] = countryCode;
			}
			else
			{
				countryCode = (string)HttpContext.Current.Session[sessionKeyName];
			}

			return countryCode;

		}

		/// <summary>
		/// Gets the Culture from the browser. Found this here:
		/// https://madskristensen.net/post/get-language-and-country-from-a-browser-in-aspnet
		/// </summary>
		/// <returns>A RegionInfo object based the users CultureInfo</returns>
		private static RegionInfo ResolveCountry()
		{
			int DEFAULT_LCID = 3081;
			CultureInfo culture = ResolveCulture();
			return new RegionInfo(culture != null ? culture.LCID : DEFAULT_LCID);
		}

		/// <summary>
		/// Gets the Culture from the browser. Found this here:
		/// https://madskristensen.net/post/get-language-and-country-from-a-browser-in-aspnet
		/// </summary>
		/// <returns>A CultureInfo object based on the user language from the browser</returns>
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

		private static OffsetDateTime GetOffsetDateTime(DateTime dateTime)
		{
			var offset = Offset.FromSeconds(-1 * UserTimeZoneOffsetMins() * 60);
			var localDateTime = LocalDateTime.FromDateTime(dateTime);
			return new OffsetDateTime(localDateTime, offset);
		}

		/// <summary>
		/// Gets and sets the time zone offset in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A numeric offset from UTC</returns>
		private static Int32 UserTimeZoneOffsetMins()
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

		/// <summary>
		/// Gets the time zone using the country code
		/// I got this code from here https://stackoverflow.com/a/24907552/4782728. I'm so grateful as it really helped.
		/// </summary>
		/// <returns>A DateTimeZone from NodaTime based on the country code.</returns>
		private static DateTimeZone GetDateTimeZoneFromCountryCode()
		{
			string countryCode = UserCountryCode();
			var CountryInfo = (from location in TzdbDateTimeZoneSource.Default.ZoneLocations
							   where location.CountryCode.Equals(countryCode,
										  StringComparison.OrdinalIgnoreCase)
							   select new { location.ZoneId, location.CountryName })
							 .FirstOrDefault();
			
			DateTimeZone TimeZone = DateTimeZoneProviders.Tzdb[CountryInfo.ZoneId];
			return TimeZone;
		}
		#endregion 
	}
}
