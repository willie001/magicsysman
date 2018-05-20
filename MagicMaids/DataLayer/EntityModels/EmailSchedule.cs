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
				return _lastScheduledDate.ToUser();;
			}
			set
			{
				var convertedValue = value.ToUTC();
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
				return _nextScheduled.ToUser();
			}
			set
			{
				var convertedValue = value.ToUTC();
				if (convertedValue != _nextScheduled)
				{
					_nextScheduled = convertedValue;
				}
			}
		}
		private DateTime _nextScheduled;

		#endregion

		#region Methods, Public
		public override string ToString()
		{
			return Schedule.ToString();
		}
		#endregion
	}
}
