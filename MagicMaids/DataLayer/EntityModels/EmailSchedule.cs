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
			get;
			set;
		}

		public DateTime NextScheduled
		{
			get;
			set;
		}

		#endregion

		#region Methods, Public
		public override string ToString()
		{
			return Schedule.ToString();
		}
		#endregion
	}
}
