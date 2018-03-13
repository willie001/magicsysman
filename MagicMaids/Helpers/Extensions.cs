using System;
using System.Web;

namespace MagicMaids
{
	public static class Extensions
	{
		/// <summary>
		/// Convert the passed datetime into client timezone (date).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToClientDate(this DateTime dt)
		{
			return GetClientDateTime(dt).ToString("d MMM yyyy");
		}

		/// <summary>
		/// Convert the passed datetime into client timezone (datetime).
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToClientDateTime(this DateTime dt)
		{
			return GetClientDateTime(dt).ToString("d MMM yyyy HH:mm:ss");
		}

		private static DateTime GetClientDateTime(DateTime dt)
		{
			var timeOffSet = HttpContext.Current.Session["timezoneoffset"];  // read the value from session

			if (timeOffSet != null)
			{
				var offset = int.Parse(timeOffSet.ToString());
				dt = dt.AddMinutes(-1 * offset);

				return dt;
			}

			// if there is no offset in session return the datetime in server timezone
			return dt.ToLocalTime();
		}
	}
}
