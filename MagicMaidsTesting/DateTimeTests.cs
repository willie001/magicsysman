using System;
using MagicMaids;
using NUnit.Framework;

namespace MagicMaidsTesting
{
	[TestFixture()]
	public class DateTimeTests
	{
		[Test()]
		public void DateTimeToMinutes_Expect_LongMins()
		{
			var hr = 14;
			var min = 30;
			var expected = (long)((hr * 60) + min);

			DateTime dt = new DateTime(2000, 1, 1, hr, min, 0, DateTimeKind.Local);
			var output = dt.ToMinutes();

			Assert.AreEqual(expected, output, "Invalid conversion to minutes");
		}

		[Test()]
		public void DateTimeToTime_Expect_DateTime()
		{
			long totalmins = 870;
			var hr = 14;
			var min = 30;

			var expected = new DateTime(2000, 1, 1, hr, min, 0, DateTimeKind.Unspecified);

			var output = totalmins.ToTime();

			Assert.AreEqual(expected, output, "Invalid conversion to datetime");
		}

		[Test()]
		public void DateTimeFuture_Expect_True()
		{
			var date = DateTime.Now;
			date = date.AddDays(1);

			Assert.IsFalse(date.IsPastDate("test"), $"{date.ToString()} is past date");

		}

		[Test()]
		public void DateTimeCurrent_Expect_True()
		{
			var date = DateTime.Now;

			Assert.IsFalse(date.IsPastDate("test"), $"{date.ToString()} is current date");

		}

		[Test()]
		public void DateTimePast_Expect_True()
		{
			var date = DateTime.Now;
			date = date.AddDays(-2);

			Assert.True(date.IsPastDate("test"), $"{date.ToString()} is past date");

		}
	}
}
