#region Using
using System;

using MagicMaids.EntityModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endregion

namespace MagicMaids.ViewModels
{
	public enum BookingStatus
	{
		AVAILABLE,
		NEW,
		PENDING,
		CONFIRMED,
		CANCELLED
	}

	public enum JobTypeEnum
	{
		Weekly,
		Fortnighly,
		OneOff,
		Vacate
	}

	public class JobBookingsVM : BaseViewModel
	{
		private long _startTime;
		private long _endTime;
		#region Constructor
		public JobBookingsVM()
		{
			JobStatus = BookingStatus.NEW;
		}

		#endregion

		#region Properties, Public
		public String Id
		{
			get;
			set;
		}

		public Boolean IsNewItem
		{
			get;
			set;
		}

		public String CleanerId
		{
			get;
			set;
		}

		public String ClientId
		{
			get;
			set;
		}

		public JobTypeEnum JobType
		{
			get;
			set;
		}

		public String JobTypeName
		{
			get
			{
				return JobType.ToString();
			}
		}

		public DateTime? JobDateUTC
		{
			get;
			set;
		}

		public String JobDateFormatted
		{
			get
			{
				if (JobDateUTC.HasValue)
				{
					return JobDateUTC.Value.FormatUserDate();
				}
				return "";
			}
		}

		public string WeekDay
		{
			get;
			set;
		}

		public Int32 TeamSize
		{
			get;
			set;
		}

		public String CleanerTeam
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_cleanerTeam))
				{
					return "";
				}

				return (_cleanerTeam.Contains(",")) ? _cleanerTeam.Replace(",", "<br/>") : _cleanerTeam;
			}
			set
			{
				_cleanerTeam = value;
			}
		}
		private String _cleanerTeam;

		public long StartTime
		{
			get => _startTime;
			set
			{
				_startTime = value;
				StartTimeForControl = _startTime.ToTime();
			}
		}

		// need to be get/set plain to allow control to set it
		public DateTime StartTimeForControl
		{
			get;
			set;
		}


		public long EndTime
		{
			get => _endTime;
			set
			{
				_endTime = value;
				EndTimeForControl = _endTime.ToTime();
			}
		}

		// need to be get/set plain to allow control to set it
		public DateTime EndTimeForControl
		{
			get;
			set;
		}

		public string StartTimeOfDay
		{
			get
			{
				return StartTime.ToTime().FormatTime();
			}
		}

		public string EndTimeOfDay
		{
			get
			{
				return EndTime.ToTime().FormatTime();
			}
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public BookingStatus JobStatus
		{
			get;
			set;
		}

		public String JobSuburb
		{
			get;
			set;
		}

		public String JobColourCode
		{
			get;
			set;
		}

		public String JobDescription
		{
			get
			{
				if (JobDateUTC != null && (JobType == JobTypeEnum.OneOff || JobType == JobTypeEnum.Vacate))
				{
					return $"{WeekDay} on {JobDateUTC.Value.FormatUserDate()} ({StartTimeOfDay}-{EndTimeOfDay})";
				}
				else
				{
					return $"{WeekDay} ({StartTimeOfDay}-{EndTimeOfDay})";
				}
			}
		}

		#endregion

		#region Methods, Public
		public void PopulateVM(Guid? cleanerId, JobBooking entityModel)
		{
			if (entityModel == null)
				return;

			if (cleanerId.Equals(Guid.Empty))
				return;

			Id = (Helpers.IsValidGuid(entityModel.Id)) ? entityModel.Id : "";

			CleanerId = entityModel.PrimaryCleanerRefId;
			ClientId = entityModel.ClientRefId;
			StartTime = entityModel.StartTime;
			EndTime = entityModel.EndTime;
			JobType = entityModel.JobType;
			JobStatus = entityModel.JobStatus;
			WeekDay = entityModel.WeekDay;
			JobDateUTC = entityModel.JobDateUTC;
			JobSuburb = entityModel.JobSuburb;
			TeamSize = entityModel.TeamSize;
		}
		#endregion
	}
}
