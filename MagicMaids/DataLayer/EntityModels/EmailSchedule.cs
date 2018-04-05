#region Using
using System;
#endregion

namespace MagicMaids.EntityModels
{
	public enum ScheduleSetting
	{
		None,
		Daily
	}

	public class EmailSchedule : BaseModel
	{

		#region Properties, Public
		public Template ScheduledDocument
		{
			get;
			set;
		}

		public ScheduleSetting Schedule
		{
			get;
			set;
		}

		public DateTime LastScheduled
		{
			get
			{
				return DateTimeWrapper.UTCtoLocal(_lastScheduledDate);
			}
			set
			{
				var convertedValue = DateTimeWrapper.LocaltoUTC(value);
				if (convertedValue != _lastScheduledDate)
				{
					_lastScheduledDate = convertedValue;
				}
			}
		}
		private DateTime _lastScheduledDate;

		public DateTime NextScheduled
		{
			get
			{
				return DateTimeWrapper.UTCtoLocal(_nexScheduled);
			}
			set
			{
				var convertedValue = DateTimeWrapper.LocaltoUTC(value);
				if (convertedValue != _nexScheduled)
				{
					_nexScheduled = convertedValue;
				}
			}
		}
		private DateTime _nexScheduled;

		#endregion

		#region Methods, Public
		public override string ToString()
		{
			return Schedule.ToString();
		}
		#endregion
	}
}
