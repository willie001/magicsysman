using System;
using System.Web;
using NLog;

namespace MagicMaids
{
	public static class Extensions
	{
		/// <summary>
		/// Convert the passed datetime into client timezone (date).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToClientDateString(this DateTime dt)
		{
			return GetClientDateTime(dt).ToString("d MMM yyyy");
		}

		/// <summary>
		/// Convert the passed datetime into client timezone (date).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime ToClientDate(this DateTime dt)
		{
			return GetClientDateTime(dt);
		}

		/// <summary>
		/// Convert the passed datetime into client timezone (datetime).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToClientDateTimeString(this DateTime dt)
		{
			return GetClientDateTime(dt).ToString("d MMM yyyy HH:mm:ss");
		}

		/// <summary>
		/// Convert the passed datetime into client timezone (datetime).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime ToClientDateTime(this DateTime dt)
		{
			return GetClientDateTime(dt);
		}

		private static DateTime GetClientDateTime(DateTime dt)
		{
			var timeOffSet = HttpContext.Current.Session["timezoneoffset"];  // read the value from session
			var debug = $"Input: {dt.ToString()} | Offset: {timeOffSet.ToString()}";

			if (timeOffSet != null)
			{
				debug += $" | Offset: {timeOffSet.ToString()}";

				var offset = int.Parse(timeOffSet.ToString());
				dt = dt.AddMinutes(-1 * offset);
				debug += $" | Output: {dt.ToString()}";

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Debug, "Error saving cleaner", nameof(ToClientDateTime), null, debug);

				return dt;
			}

			// if there is no offset in session return the datetime in server timezone
			return dt.ToLocalTime();
		}
	}
}
