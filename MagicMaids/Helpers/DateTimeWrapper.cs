using NodaTime;
using NodaTime.Calendars;
using NodaTime.Extensions;

using System;
using System.Globalization;
using System.Text;
using System.Web;

namespace MagicMaids
{
	public static class DateTimeWrapper
	{
		[Flags]
		private enum DateTimeParts
		{
			Day = 0x01,
			Month = 0x02,
			Year = 0x04,
		}

		/// <summary>
		/// NodaTime's DateTime.Now
		/// </summary>
		/// <value>The now.</value>
		internal static Instant NowInstance
		{
			get
			{
				return SystemClock.Instance.GetCurrentInstant();
			}
		}

		internal static DateTime NowUtc
		{
			get
			{
				return NowInstance.ToDateTimeUtc();
			}
		}


		public static string DisplayLocalNow()
		{
			return NowUtc.ToUser().FormatUserDateTime();
		}

		public static string DisplayUtcNow()
		{
			return NowUtc.FormatUserDateTime();
		}

		/// <summary>
		/// Converts a non-local-time DateTime to a local-time DateTime based on the
		/// specified timezone. The returned object will be of Unspecified DateTimeKind 
		/// which represents local time agnostic to servers timezone. To be used when
		/// we want to convert UTC to local time somewhere in the world.
		/// </summary>
		/// <param name="dateTime">Non-local DateTime as UTC or Unspecified DateTimeKind.</param>
		/// <returns>Local DateTime as Unspecified DateTimeKind.</returns>
		internal static DateTime ToUser(this DateTime utcDateTime)
		{
			if (utcDateTime.Kind == DateTimeKind.Local)
			{
				return utcDateTime;
			}
			else if (utcDateTime.Kind == DateTimeKind.Unspecified)
			{
				utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
			}

			Instant instant = utcDateTime.ToInstant();
			DateTimeZone zone = CultureHelper.GetDateTimeZone();
			ZonedDateTime inZone = instant.InZone(zone);

			return inZone.ToDateTimeUnspecified();
		}

		/// <summary>
		/// Converts a local-time DateTime to UTC DateTime based on the specified
		/// timezone. The returned object will be of UTC DateTimeKind. To be used
		/// when we want to know what's the UTC representation of the time somewhere
		/// in the world.
		/// </summary>
		/// <param name="dateTime">Local DateTime as UTC or Unspecified DateTimeKind.</param>
		/// <returns>UTC DateTime as UTC DateTimeKind.</returns>
		internal static DateTime ToUTC(this DateTime localDateTime)
		{
			if (localDateTime.Kind == DateTimeKind.Local)
			{
				return localDateTime.ToUniversalTime();
			}

			DateTimeZone zone = CultureHelper.GetDateTimeZone();
			LocalDateTime asLocal = localDateTime.ToLocalDateTime();
			ZonedDateTime asZoned = asLocal.InZoneLeniently(zone);
			Instant instant = asZoned.ToInstant();
			ZonedDateTime asZonedInUtc = instant.InUtc();

			return asZonedInUtc.ToDateTimeUtc();
		}

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
			var newDate = dt.ToString("yyyy-MM-dd HH:mm:ss.ffff");
			return newDate;
		}

		public static string FormatDatabaseDate(this DateTime dt)
		{
			var newDate = dt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
			return newDate;
		}

		public static string FormatTime(this DateTime dt)
		{
			var newDate = dt.ToString("HH:mm", CultureInfo.InvariantCulture);
			return newDate;
		}

		public static OffsetDateTime GetOffsetDateTime(this DateTime dateTime)
		{
			var offset = Offset.FromSeconds(-1 * CultureHelper.UserTimeZoneOffsetMins() * 60);
			var localDateTime = LocalDateTime.FromDateTime(dateTime);
			return new OffsetDateTime(localDateTime, offset);
		}

		public static long ToMinutes(this DateTime serverDatetime)
		{
			var _dt = serverDatetime;
			LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - kind", _dt.Kind.ToString());
			if (_dt.Kind == DateTimeKind.Local)
			{
				LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - Local", _dt.ToString());

				//_dt = DateTime.SpecifyKind(_dt, DateTimeKind.Unspecified);
				var _dtUtc = _dt.ToUTC();

				LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - UTC", _dtUtc.ToString());
				// TimePicker changes new times to Local Kind but it is UTC.
				_dt = _dtUtc.ToUser();
				LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - User", _dt.ToString());
			}

			var _hr = _dt.Hour;
			var _min = _dt.Minute;

			return (long)((_hr * 60) + _min);
		}

		public static DateTime ToTime(this long timeMins)
		{
			return Convert.ToInt32(timeMins).ToTime();
		}

		public static DateTime ToTime(this Int32 timeMins)
		{
			var _hr = timeMins / 60;
			var _min = timeMins - (_hr * 60);

			var _dt = new DateTime(2000, 1, 1, _hr, _min, 0);
			_dt = DateTime.SpecifyKind(_dt, DateTimeKind.Unspecified);
			return _dt;
		}

		public static String ToTimeDuration(this Int32 timeMins)
		{
			var _hr = timeMins / 60;
			var _min = timeMins - (_hr * 60);

			if (_hr == 0)
			{
				return $"{_min} minutes";
			}

			if (_hr == 1)
			{
				return $"1 hour {_min} minutes";
			}

			if (_min == 0)
			{
				return $"{_hr} hours";
			}

			return $"{_hr} hours {_min} minutes";
		}

		public static bool IsPastDate(this DateTime compareDate, string callingMethod)
		{
			var localNow = DateTimeWrapper.NowUtc.Date.ToLocalDateTime();
			var localCompare = compareDate.ToLocalDateTime();
			var compareDiff = (compareDate.Date - DateTimeWrapper.NowUtc.Date).TotalDays;
			var comparePeriod = Period.Between(localNow, localCompare, PeriodUnits.Days);

			LogHelper.LogDebugDetails(callingMethod, $"Compare Date: {localCompare}", $"Compare ToUser: {compareDate.ToUser().Date}",
								  $"Now (UTC): {DateTimeWrapper.NowUtc.Date}",
		                          $"Start Kind: {compareDate.Kind.ToString()}", 
								  $"Now Kind:  {DateTimeWrapper.NowUtc.Kind.ToString()}", 
			                          $"Start Period: {comparePeriod.Days}");

			return (comparePeriod.Days < 0);
		}

		public static string WeekYearStyle(this DateTime matchDate, Boolean nextWeek = false) {
			if (matchDate < DateTime.MinValue || matchDate > DateTime.MaxValue)
			{
				return "";
			}

			// We'll use BCL, but if the date is before first monday I'll force it to week 1
			//https://nodatime.org/2.2.x/userguide/weekyears
			//var utcDate = matchDate.ToUTC(); This will select the previous day and set the wrong week for Mondays

            if (nextWeek) { matchDate = matchDate.AddDays(7); }

			CalendarWeekRule weekRule = CalendarWeekRule.FirstDay;
			DayOfWeek firstWeekDay = DayOfWeek.Monday;
			Calendar calendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;

			int currentWeek = calendar.GetWeekOfYear(matchDate, weekRule, firstWeekDay);

			if (currentWeek % 2 == 0)
			{
				return NamedColours.WeeksEven;
			}

			return NamedColours.WeeksOdd;
		}

        public static string WeekYearStyle(this DateTime? matchDateUTC)
        {
            if (matchDateUTC == null || matchDateUTC < DateTime.MinValue || matchDateUTC > DateTime.MaxValue)
            {
                return "";
            }

            // We'll use BCL, but if the date is before first monday I'll force it to week 1
            //https://nodatime.org/2.2.x/userguide/weekyears
            var utcDate = DateTime.Parse(matchDateUTC.ToString());

            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay;
            DayOfWeek firstWeekDay = DayOfWeek.Monday;
            Calendar calendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;

            int currentWeek = calendar.GetWeekOfYear(utcDate, weekRule, firstWeekDay);

            if (currentWeek % 2 == 0)
            {
                return NamedColours.WeeksEven;
            }

            return NamedColours.WeeksOdd;
        }

        public static DayOfWeek ToDayOfWeek(this string dayOfWeek)
		{
			if (Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
			{
				DayOfWeek equivalentDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayOfWeek, true);
				return equivalentDay;
			}
			else
			{
				throw new ArgumentException("Invalid week day");
			}
		}

		public static DateTime FindNextDateForDay(string dayOfWeek)
		{
			return FindNextDateForDay(dayOfWeek.ToDayOfWeek());
		}

		public static DateTime FindNextDateForDay(DayOfWeek WeekDay)
		{
            var _date = DateTime.Now; //DateTime.Now.ToUTC();
			if (_date.DayOfWeek == WeekDay)
			{
				return _date;
			}

			int daysToAdd = ((int)WeekDay - (int)_date.DayOfWeek + 7) % 7;
			return _date.AddDays(daysToAdd);
		}

		public static bool IsDayInRange(this DayOfWeek WeekDay, DateTime startDate, DateTime endDate)
		{
			// Todo fix for sunday conversion
			var dayValue = (int)WeekDay;
			if (dayValue == 7)
				dayValue = 0;

			var correctedDayOfWeek = (DayOfWeek)dayValue;

			if (startDate == default(DateTime) || endDate == default(DateTime))
			{
				return false;
			}

			if (endDate < startDate)
			{
				return false;
			}

			DateTime dt = startDate.ToUser();
			while(dt <= endDate.ToUser())
			{
				if (dt.DayOfWeek.Equals(correctedDayOfWeek))
				{
					return true;
				}
				dt = dt.AddDays(1);
			}

			return false;
		}

		public static string FormatDateRange(DateTime startDate, DateTime endDate)
		{

			if (endDate.Date < startDate.Date)
			{
				endDate = startDate;
			}

			string result;
			if (startDate.Date == endDate.Date)
			{
				result = FormatDateTime(startDate, DateTimeParts.Day | DateTimeParts.Month | DateTimeParts.Year);
			}
			else if (startDate.Year == endDate.Year && startDate.Month == endDate.Month)
			{
				result = $"{FormatDateTime(startDate, DateTimeParts.Day)}-{FormatDateTime(endDate, DateTimeParts.Day | DateTimeParts.Month | DateTimeParts.Year)}";
			}
			else if (startDate.Year == endDate.Year)
			{
				result = $"{FormatDateTime(startDate, DateTimeParts.Day | DateTimeParts.Month)}-{FormatDateTime(endDate, DateTimeParts.Day | DateTimeParts.Month | DateTimeParts.Year)}";
			}
			else
			{
				result = $"{FormatDateTime(startDate, DateTimeParts.Day | DateTimeParts.Month | DateTimeParts.Year)}-{FormatDateTime(endDate, DateTimeParts.Day | DateTimeParts.Month | DateTimeParts.Year)}";
			}

			return  result;
		}

		/// <summary>
		/// Formats a single date/time value using the current settings.
		/// </summary>
		private static string FormatDateTime(DateTime dt, DateTimeParts parts)
		{
			StringBuilder sb = new StringBuilder();

			if (parts.HasFlag(DateTimeParts.Day))
			{
				if (parts.HasFlag(DateTimeParts.Month))
				{
					sb.Append("d MMM");
				}
				else sb.Append("%d");
			}
			if (parts.HasFlag(DateTimeParts.Year))
				sb.Append(" yyyy");

			return dt.ToString(sb.ToString());
		}

	}

}
