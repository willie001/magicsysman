using NodaTime;
using NodaTime.Extensions;

using System;
using System.Globalization;
using System.Web;

namespace MagicMaids
{
	public static class DateTimeWrapper
	{
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
			var newDate = dt.ToString("yyyy-MM-dd HH:mm:ss:ffff");
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
			if (_dt.Kind == DateTimeKind.Local)
			{
				// DateTime picker changes new times to Unspecified.
				_dt = DateTime.SpecifyKind(_dt, DateTimeKind.Unspecified);
			}

			var _hr = _dt.Hour;
			var _min = _dt.Minute;

			return (long)((_hr * 60) + _min);
		}

		public static DateTime ToTime(this long timeMins)
		{
			var _hr = Convert.ToInt32(timeMins/60);
			var _min = (int)timeMins - (_hr*60);

			var _dt = new DateTime(2000, 1, 1, _hr, _min, 0);
			_dt = DateTime.SpecifyKind(_dt, DateTimeKind.Unspecified);
			return _dt;
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
	}
}
