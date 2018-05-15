using System;
using System.Globalization;
using System.Web;
using NodaTime;

namespace MagicMaids
{
	public class DateTimeWrapper
	{
		private static DateTimeZone UserTimeZone { get; set; }
		private static Int32 TimeZoneOffsetMins;
		public static String TimeZoneName;

		public static string DisplayUserTimeZone
		{
			get
			{
				return UserTimeZone.ToString();	
			}	
		}

		public static string DisplayTimeZoneOffsetMins
		{
			get
			{
				return TimeZoneOffsetMins.ToString();
			}
		}

		public static string FormatClientDate(DateTime dt)
		{
			return dt.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
		}

		public static string FormatLocalNow()
		{
			return DateTimeWrapper.LocalNow.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
		}

		public static string FormatClientDateTime(DateTime dt)
		{
			return dt.ToString("d MMM yyyy HH:mm:ss");
		}

		public static string FormatDateTimeForDatabase(DateTime dt)
		{
			return dt.ToString("yyyy-MM-dd HH:mm:ss:ff");
		}

		public static Instant Now
		{
			get
			{
				return SystemClock.Instance.GetCurrentInstant();
			}
		}

		public static LocalDateTime LocalNow 
		{
			get{
				if (UserTimeZone == null)
				{
					SetTimeZoneFromContext();
				}
				return Now.InZone(UserTimeZone).LocalDateTime;
			}
		}

		public static DateTime UTCtoLocal(DateTime dateTime)
		{
			if (dateTime == null || !SetTimeZoneFromContext() || dateTime.GetType() != typeof(DateTime))
			{
				return DateTime.MinValue;
			}

			// If should be set to DateTimeKind.Unspecified in Convert
			var dateKind = ((DateTime)dateTime).Kind;
			if (dateKind == DateTimeKind.Local || dateKind == DateTimeKind.Unspecified)
			{
				return dateTime;
			}

			Instant instant = Instant.FromDateTimeUtc(dateTime);
			ZonedDateTime usersZonedDateTime;
			if (String.IsNullOrWhiteSpace(TimeZoneName))
			{
				var offset = Offset.FromSeconds(-1*TimeZoneOffsetMins * 60);
				return instant.WithOffset(offset).LocalDateTime.ToDateTimeUnspecified();
			}
			else
			{
				usersZonedDateTime = instant.InZone(UserTimeZone);
				return usersZonedDateTime.ToDateTimeUnspecified();
			}
		}

		public static DateTime LocaltoUTC(DateTime dateTime)
		{
			if (dateTime == null || !SetTimeZoneFromContext() || dateTime.GetType() != typeof(DateTime))
			{
				return DateTime.MinValue;
			}

			if (String.IsNullOrWhiteSpace(TimeZoneName) || TimeZoneName.GetType() != typeof(string) || !DateTimeZoneProviders.Tzdb.Ids.Contains(TimeZoneName))
			{
				return dateTime;
			}

			ZonedDateTime zonedDbDateTime;
			if (String.IsNullOrWhiteSpace(TimeZoneName))
			{
				return GetOffsetDateTime(dateTime).ToInstant().ToDateTimeUtc();
			}
			else
			{
				var localDateTime = LocalDateTime.FromDateTime((DateTime)dateTime);
				zonedDbDateTime = UserTimeZone.AtLeniently(localDateTime);
				return zonedDbDateTime.ToDateTimeUtc();
			}

		}

		private static OffsetDateTime GetOffsetDateTime(DateTime dateTime)
		{
			var offset = Offset.FromSeconds(-1 * TimeZoneOffsetMins * 60);
			var localDateTime = LocalDateTime.FromDateTime(dateTime);
			return new OffsetDateTime(localDateTime, offset);
		}

		private static Boolean  SetTimeZoneFromContext()
		{
			if (!String.IsNullOrWhiteSpace(TimeZoneName) && UserTimeZone != null)
			{
				return true;
			}

			var timeOffSet = HttpContext.Current.Session["timezoneoffset"]?.ToString();  // read the value from session
			TimeZoneName = HttpContext.Current.Session["timezonename"]?.ToString();  // read the value from session

			if (String.IsNullOrWhiteSpace(TimeZoneName))
			{
				TimeZoneName = "";
				UserTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC");

			}
			else 
			{
				TimeZoneName = TimeZoneName.Replace("%2F", "/");
				if (DateTimeZoneProviders.Tzdb.Ids.Contains(TimeZoneName))
				{
					UserTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZoneName);
				}
				else
				{
					TimeZoneName = "";
					UserTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC");
				}
			}

			if (!String.IsNullOrWhiteSpace(timeOffSet))
			{
				Int32.TryParse(timeOffSet, out TimeZoneOffsetMins);
			}

			return true;
		}
	}
}
