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

		#region Methods, Public
		public static string FormatUserDate(this DateTime dt)
		{
			return dt.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
		}

		public static string FormatUserDateTime(this DateTime dt)
		{
			return dt.ToString("d MMM yyyy HH:mm:ss");
		}

		public static string FormatDatabaseDateTime(this DateTime dt)
		{
			return dt.ToString("yyyy-MM-dd HH:mm:ss:ff");
		}

		public static string DisplayCultureSettings(string seperator = "\n")
		{
			StringBuilder output = new StringBuilder();

			DateTime _serverDateTime = DateTime.Now;
			Instant _nodeDateTime = CultureHelper.Now;

			output.Append($"FROM SERVER LOCATION:{seperator}");
			output.Append($"Current server DateTime (DateTime.Now): {_serverDateTime.ToString()}{seperator}");
			output.Append($"Current NodeTime DateTime (CultureHelper.Now): {_nodeDateTime.ToString()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"UTC:{seperator}");
			output.Append($"UTC for user's current date time (CultureHelper.ToUTC: {_serverDateTime.ToUTC()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"FROM USER LOCATION (BROWSER):{seperator}");
			output.Append($"Current time at user location (CultureHelper.ToLocal Extension): {_serverDateTime.ToLocalTime()}{seperator}");

			output.Append($"{seperator}");

			output.Append($"FROM BROWSER:{seperator}");
			output.Append($"User Country Code: {CultureHelper.UserCountryCode()}{seperator}");
			output.Append($"User Timezone Name: {CultureHelper.UserTimeZoneName()}{seperator}");
			output.Append($"User Timezone Offset in minutes: {CultureHelper.UserTimeZoneOffsetMins().ToString()}{seperator}");

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
			return DateTime.Now.ToLocal().FormatUserDateTime();
		}

		public static DateTime ToLocal(this DateTime dateTime)
		{
			string timeZone = UserTimeZoneName();

			return dateTime.ToLocal(timeZone);
		}

		public static DateTime ToUTC(this DateTime dateTime)
		{
			string timeZone = UserTimeZoneName();
			return dateTime.ToUTC(timeZone);
		}

		/// <summary>
		/// Converts a non-local-time DateTime to a local-time DateTime based on the
		/// specified timezone. The returned object will be of Unspecified DateTimeKind 
		/// which represents local time agnostic to servers timezone. To be used when
		/// we want to convert UTC to local time somewhere in the world.
		/// </summary>
		/// <param name="dateTime">Non-local DateTime as UTC or Unspecified DateTimeKind.</param>
		/// <param name="timezone">Timezone name (in TZDB format).</param>
		/// <returns>Local DateTime as Unspecified DateTimeKind.</returns>
		public static DateTime ToLocal(this DateTime dateTime, string timezone)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				return dateTime;
			}

			var zone = DateTimeZoneProviders.Tzdb[timezone];
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
		public static DateTime ToUTC(this DateTime dateTime, string timezone)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				return dateTime.ToUniversalTime();
			}

			var zone = DateTimeZoneProviders.Tzdb[timezone];
			LocalDateTime asLocal = dateTime.ToLocalDateTime();
			ZonedDateTime asZoned = asLocal.InZoneLeniently(zone);
			Instant instant = asZoned.ToInstant();
			ZonedDateTime asZonedInUtc = instant.InUtc();
			DateTime utc = asZonedInUtc.ToDateTimeUtc();

			return utc;
		}

		//public static DateTime ConvertLocateDateToUTC(this DateTime dateTime)
		//{
		//	ZonedDateTime zonedDbDateTime;
		//	var newDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 12, 0, 0);
		//	return newDate.ConvertLocateDateToUTC();
		//}


		///// <summary>
		///// An extension method for DateTime. It converts a Utc DateTime to a local date time using the user's time zone
		///// </summary>
		///// <param name="utcDateTime">The datetime object which the extension method is called from</param>
		///// <param name="cachingTimeInMins">Caching time in minutes, used so you don't need to keep getting the timezone every time.</param>
		///// <returns>A DateTime object local to the user</returns>
		//public static DateTime ConvertUtcToLocalDateTime(this DateTime utcDateTime)
		//{
		//	IAppCache cache = new CachingService();

		//	//I got this line from StackOverflow. It helped a lot at the end. https://stackoverflow.com/a/41662352/4782728
		//	var newUtcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
		//	DateTimeZone TimeZone = cache.GetOrAdd($"dateTimeZone-{UserCountryCode()}", () => GetDateTimeZone(), new TimeSpan(0, 30, 0));

		//	DateTime objdate = Instant.FromDateTimeUtc(newUtcDateTime)
		//			  .InZone(TimeZone)
		//			  .ToDateTimeUnspecified();
		//	return objdate;
		//}
		#endregion 

		#region Methods, Private
		private static OffsetDateTime GetOffsetDateTime(DateTime dateTime)
		{
			var offset = Offset.FromSeconds(-1 * UserTimeZoneOffsetMins() * 60);
			var localDateTime = LocalDateTime.FromDateTime(dateTime);
			return new OffsetDateTime(localDateTime, offset);
		}

		/// <summary>
		/// Gets and sets the time zone name in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A two letter country code</returns>
		private static string UserTimeZoneName()
		{
			const string sessionKeyName = "timezonename";
			string timeZoneName = "";

			if (HttpContext.Current.Session[sessionKeyName] == null)
			{
				timeZoneName = ResolveCountry().ToString();
				HttpContext.Current.Session[sessionKeyName] = timeZoneName;
			}
			else
			{
				timeZoneName = (string)HttpContext.Current.Session[sessionKeyName];
			}

			if (timeZoneName.Contains("%2F"))
			{
				timeZoneName = timeZoneName.Replace("%2F", "/");
				HttpContext.Current.Session[sessionKeyName] = timeZoneName;
			}

			return timeZoneName;

		}

		/// <summary>
		/// Gets and sets the time zone offset in the session to save keep resolving it from the browser every time.
		/// </summary>
		/// <returns>A numeric offset from UTC</returns>
		private static Int32 UserTimeZoneOffsetMins()
		{
			const string sessionKeyName = "timezoneoffset";
			string timezoneOffset = "0";


			if (HttpContext.Current.Session[sessionKeyName] == null)
			{
				timezoneOffset = ResolveCountry().ToString();
				HttpContext.Current.Session[sessionKeyName] = timezoneOffset;
			}
			else
			{
				timezoneOffset = (string)HttpContext.Current.Session[sessionKeyName];
			}

			Int32 offset = 0;
			if (Int32.TryParse(timezoneOffset, out offset))
			{
				return offset;
			}

			return 0;
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

		/// <summary>
		/// Calls the method for getting the TimeZone from the country code, but sets the country code before calling it. 
		/// Helps to do this when using caching and the delegate method would have had parameters.
		/// </summary>
		/// <returns>A DateTimeZone from NodaTime based on the country code.</returns>
		private static DateTimeZone GetDateTimeZone()
		{
			return GetDateTimeZoneFromCountryCode(UserCountryCode());
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
		/// Gets the time zone using the country code
		/// I got this code from here https://stackoverflow.com/a/24907552/4782728. I'm so grateful as it really helped.
		/// </summary>
		/// <param name="countryCode">Two digit country code.</param>
		/// <returns>A DateTimeZone from NodaTime based on the country code.</returns>
		private static DateTimeZone GetDateTimeZoneFromCountryCode(string countryCode)
		{
			var CountryInfo = (from location in TzdbDateTimeZoneSource.Default.ZoneLocations
							   where location.CountryCode.Equals(countryCode,
										  StringComparison.OrdinalIgnoreCase)
							   select new { location.ZoneId, location.CountryName })
							 .FirstOrDefault();
			DateTimeZone TimeZone = DateTimeZoneProviders.Tzdb[CountryInfo.ZoneId];

			if (TimeZone == null)
			{
				TimeZone = ResolveUserTimeZone();
			}

			return TimeZone;
		}

		private static DateTimeZone ResolveUserTimeZone()
		{
			DateTimeZone _userTZ = null;
			if (String.IsNullOrWhiteSpace(UserTimeZoneName()))
			{
				_userTZ = DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC");
			}
			else
			{
				if (DateTimeZoneProviders.Tzdb.Ids.Contains(UserTimeZoneName()))
				{
					_userTZ = DateTimeZoneProviders.Tzdb.GetZoneOrNull(UserTimeZoneName());
				}
				else
				{
					_userTZ = DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC");
				}
			}

			return _userTZ;
		}

		#endregion 
	}
}
